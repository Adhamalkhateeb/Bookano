using Bookano.Application.DTOs.Rentals;
using Bookano.Application.Services.Rentals;
using Bookano.Web.ViewModels.BookCopies;
using Bookano.Web.ViewModels.Rentals;
using Microsoft.AspNetCore.DataProtection;

namespace Bookano.Web.Controllers
{


    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController(
        IMapper mapper,
        IRentalService rentalService,
        IDataProtectionProvider dataProtector
    ) : Controller
    {
        private readonly IDataProtector _dataProtector = dataProtector.CreateProtector("security");
        private readonly IMapper _mapper = mapper;
        private readonly IRentalService _rentalService = rentalService;

        public async Task<IActionResult> Create(string subscriberKey, CancellationToken ct)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(subscriberKey));
            var result = await _rentalService.GetAvailableCopiesCountAsync(subscriberId,ct: ct);

            if (!result.IsSuccess)
            {
                if (
                    result.ErrorMessage == Error.BlackListedSubscriber
                    || result.ErrorMessage == Error.InactiveSubscriber
                    || result.ErrorMessage == Error.MaxAllowedCopiesReached
                )
                {
                    return View("NotAllowedRental", result.ErrorMessage);
                }

                return NotFound();
            }

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = subscriberKey,
                MaxAllowedCopies = result.Value
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RentalFormViewModel model, CancellationToken ct)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            if (!ModelState.IsValid)
            {
                await PopulateRentalFormAsync(subscriberId,model,ct);
                return View("Form", model);
            }

            var dto = _mapper.Map<RentalFormDto>(model);
            dto.SubscriberId = subscriberId;

            var result = await _rentalService.CreateAsync(dto, ct);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage is not null)
                    return View("NotAllowedRental", result.ErrorMessage);

                result.AddToModelState(ModelState);

                await PopulateRentalFormAsync(subscriberId, model, ct);
                return View("Form", model);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var rental = await _rentalService.GetDetailsAsync(id, ct);

            if (rental is null || rental.CreatedOnUtc.Date != DateTime.UtcNow.Date)
                return NotFound();

            var result = await _rentalService.GetAvailableCopiesCountAsync(rental.SubscriberId,rental.Id, ct);

            if (!result.IsSuccess)
            {
                if (
                    result.ErrorMessage == Error.BlackListedSubscriber
                    || result.ErrorMessage == Error.InactiveSubscriber
                    || result.ErrorMessage == Error.MaxAllowedCopiesReached
                )
                {
                    return View("NotAllowedRental", result.ErrorMessage);
                }

                return NotFound();
            }

            var selectedCopies = rental.RentalCopies.Select(rc => rc.BookCopy!.SerialNumber).ToList();

            var viewModel = new RentalFormViewModel
            {
                Id = rental.Id,
                SubscriberKey = _dataProtector.Protect(rental.SubscriberId.ToString()),
                SelectedCopies = selectedCopies,
                MaxAllowedCopies = result.Value
            };

            viewModel.CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(
                await _rentalService.GetCopiesForDisplayAsync(selectedCopies, ct)
            );

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RentalFormViewModel model, CancellationToken ct)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            if (!ModelState.IsValid)
            {
                await PopulateRentalFormAsync(subscriberId, model, ct);

                return View("Form", model);
            }

            var dto = _mapper.Map<RentalFormDto>(model);
            dto.SubscriberId = subscriberId;

            var result = await _rentalService.UpdateAsync(dto, ct);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage is not null)
                    return View("NotAllowedRental", result.ErrorMessage);

                result.AddToModelState(ModelState);

                await PopulateRentalFormAsync(subscriberId, model, ct);
                return View("Form", model);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        public async Task<IActionResult> Return(int id, CancellationToken ct)
        {
            var result = await _rentalService.GetReturnFormAsync(id, ct);

            if (!result.IsSuccess)
                return NotFound();

            return View("Return", _mapper.Map<RentalReturnFormViewModel>(result.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Return(RentalReturnFormViewModel model, CancellationToken ct)
        {
            var dto = _mapper.Map<RentalReturnDto>(model);
            var result = await _rentalService.ReturnAsync(dto, ct);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage is not null)
                    return View("NotAllowedRental", result.ErrorMessage);

                result.AddToModelState(ModelState);

                var formResult = await _rentalService.GetReturnFormAsync(model.Id, ct);
                if (formResult.IsSuccess)
                {
                    formResult.Value!.PenalityPaid = model.PenalityPaid;
                    
                    foreach (var modelCopy in model.RentalCopies)
                    {
                        var target = formResult.Value.RentalCopies.SingleOrDefault(rc => rc.BookCopy?.Id == modelCopy.BookCopy?.Id);
                        if (target is not null)
                            target.IsReturned = modelCopy.IsReturned;
                    }

                    return View(_mapper.Map<RentalReturnFormViewModel>(formResult.Value));
                }

                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        [HttpPost]
        public async Task<IActionResult> GetCopyDetails(SearchFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _rentalService.GetCopyDetailsAsync(model.Value, ct);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage == Error.InvalidSerialNumber)
                    return NotFound(Error.InvalidSerialNumber);

                return BadRequest(result.ErrorMessage);
            }

            var viewModel = _mapper.Map<BookCopyViewModel>(result.Value);

            return PartialView("_CopyDetails", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            var result = await _rentalService.CancelAsync(id, ct);

            if (!result.IsSuccess)
                return NotFound();

            return Ok(result.Value);
        }

        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var rental = await _rentalService.GetDetailsAsync(id, ct);

            if (rental is null)
                return NotFound();

            var viewModel = _mapper.Map<RentalViewModel>(rental);
            return View(viewModel);
        }

        private async Task PopulateRentalFormAsync(int subscriberId,RentalFormViewModel model,CancellationToken ct)
        {
            var result = await _rentalService.GetAvailableCopiesCountAsync(subscriberId,model.Id, ct);
            if (result.IsSuccess)
                model.MaxAllowedCopies = result.Value;

            model.CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(
                await _rentalService.GetCopiesForDisplayAsync(model.SelectedCopies, ct)
            );
        }

      
    }
}

