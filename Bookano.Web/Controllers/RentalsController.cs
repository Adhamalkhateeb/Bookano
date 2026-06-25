using Bookano.Application.DTOs.Rentals;
using Bookano.Application.Services.BookCopies;
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
        IRentalValidationService validationService,
        IBookCopiesService bookCopiesService,
        IDataProtectionProvider protector
    ) : Controller
    {
        private readonly IMapper _mapper = mapper;
        private readonly IRentalService _rentalService = rentalService;
        private readonly IRentalValidationService _validationService = validationService;
        private readonly IBookCopiesService _bookCopiesService = bookCopiesService;
        private readonly IDataProtector _protector = protector.CreateProtector("sec");



        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var rental = await _rentalService.GetDetailsAsync(id, ct);

            if (rental is null)
                return NotFound();
      
            return View(_mapper.Map<RentalViewModel>(rental));
        }

        [HttpGet]
        public async Task<IActionResult> Create(string subscriberKey, CancellationToken ct)
        {
            var subscriberId = int.Parse(_protector.Unprotect(subscriberKey)); 
            var result = await _validationService.CheckSubscriberEligibilityAsync(subscriberId, null, ct);

            var errorResult = HandleEligibilityError(result);
            if (errorResult is not null)
                return errorResult;

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
            var subscriberId = int.Parse(_protector.Unprotect(model.SubscriberKey));

            if (!ModelState.IsValid)
                return await FormErrorViewAsync(subscriberId, model, ct);

            var dto = _mapper.Map<RentalSaveDto>(model);
            dto.SubscriberId = subscriberId;

            var result = await _rentalService.CreateAsync(dto, ct);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage is not null)
                    return View("NotAllowedRental", result.ErrorMessage);

                result.AddToModelState(ModelState);
                return await FormErrorViewAsync(subscriberId, model, ct);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var rental = await _rentalService.GetDetailsAsync(id, ct);

            if (rental is null || rental.CreatedOnUtc.Date != DateTime.UtcNow.Date)
                return NotFound();

            var result = await _validationService.CheckSubscriberEligibilityAsync(rental.SubscriberId,rental.Id, ct);

            var errorResult = HandleEligibilityError(result);
            if (errorResult is not null)
                return errorResult;

            var selectedCopies = rental.RentalCopies.Select(rc => rc.BookCopy!.SerialNumber).ToList();

            var viewModel = new RentalFormViewModel
            {
                Id = rental.Id,
                SubscriberKey = _protector.Protect(rental.SubscriberId.ToString()),
                SelectedCopies = selectedCopies,
                MaxAllowedCopies = result.Value
            };

            viewModel.CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(
                await _bookCopiesService.GetCopiesBySerialNumbersAsync(selectedCopies, ct)
            );

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RentalFormViewModel model, CancellationToken ct)
        {
            var subscriberId = int.Parse(_protector.Unprotect(model.SubscriberKey));

            if (!ModelState.IsValid)
                return await FormErrorViewAsync(subscriberId, model, ct);

            var dto = _mapper.Map<RentalSaveDto>(model);
            dto.SubscriberId = subscriberId;

            var result = await _rentalService.UpdateAsync(dto, ct);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage is not null)
                    return View("NotAllowedRental", result.ErrorMessage);

                result.AddToModelState(ModelState);
                return await FormErrorViewAsync(subscriberId, model, ct);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value });
        }


        public async Task<IActionResult> Return(int id, CancellationToken ct)
        {
            var viewModel = await BuildRentalReturnFormViewModelAsync(id, ct);

            if (viewModel is null)
                return NotFound();

            return View("Return", viewModel);
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

                var viewModel = await BuildRentalReturnFormViewModelAsync(model.Id, ct);
                if (viewModel is not null)
                {
                    viewModel.PenalityPaid = model.PenalityPaid;
                    
                    foreach (var modelCopy in model.RentalCopies)
                    {
                        var target = viewModel.RentalCopies.SingleOrDefault(rc => rc.BookCopy?.Id == modelCopy.BookCopy?.Id);
                        if (target is not null)
                            target.IsReturned = modelCopy.IsReturned;
                    }

                    return View(viewModel);
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

            var result = await _rentalService.GetCopyReadyForRentalAsync(model.Value, ct);

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


        private async Task PopulateRentalFormAsync(int subscriberId,RentalFormViewModel model,CancellationToken ct)
        {
            var result = await _validationService.CheckSubscriberEligibilityAsync(subscriberId,model.Id, ct);
            if (result.IsSuccess)
                model.MaxAllowedCopies = result.Value;

            model.CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(
                await _bookCopiesService.GetCopiesBySerialNumbersAsync(model.SelectedCopies, ct)
            );
        }

        private async Task<RentalReturnFormViewModel?> BuildRentalReturnFormViewModelAsync(int id, CancellationToken ct)
        {
            var rental = await _rentalService.GetDetailsAsync(id, ct);

            if (rental is null || rental.CreatedOnUtc.Date == DateTime.UtcNow.Date)
                return null;

            var allowExtendResult = await _validationService.CanExtendRentalAsync(id, ct);
            var delayResult = await _validationService.CalculateReturnPenaltyAsync(id, ct);

            return new RentalReturnFormViewModel
            {
                Id = rental.Id,
                PenalityPaid = rental.PenaltyPaid,
                AllowExtend = allowExtendResult.IsSuccess && allowExtendResult.Value,
                TotalDelayInDays = delayResult.IsSuccess ? delayResult.Value : 0,
                RentalCopies = _mapper.Map<IList<RentalCopyViewModel>>(
                    rental.RentalCopies.Where(rc => !rc.ReturnDate.HasValue).ToList()
                )
            };
        }

        private IActionResult? HandleEligibilityError(Result<int> result)
        {
            if (result.IsSuccess) return null;

            if (result.ErrorMessage == Error.BlackListedSubscriber
                || result.ErrorMessage == Error.InactiveSubscriber
                || result.ErrorMessage == Error.MaxAllowedCopiesReached)
            {
                return View("NotAllowedRental", result.ErrorMessage);
            }

            return NotFound();
        }

        private async Task<IActionResult> FormErrorViewAsync(int subscriberId, RentalFormViewModel model, CancellationToken ct)
        {
            await PopulateRentalFormAsync(subscriberId, model, ct);
            return View("Form", model);
        }


    }
}

