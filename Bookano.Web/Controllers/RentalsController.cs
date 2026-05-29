using System.Linq.Dynamic.Core;
using Bookano.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDataProtectionProvider dataProtector,
        IValidator<RentalReturnFormViewModel> validator
    ) : Controller
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDataProtector _dataProtector = dataProtector.CreateProtector("security");
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<RentalReturnFormViewModel> _validator = validator;

        public async Task<IActionResult> Create(string subscriberKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(subscriberKey));

            var subscriber = await _unitOfWork
                .Subscribers.GetQueryable()
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                    .ThenInclude(r => r.RentalCopies)
                .SingleOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, availableCopiesCount) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = subscriberKey,
                MaxAllowedCopies = availableCopiesCount,
            };
            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var subscriber = await _unitOfWork
                .Subscribers.GetQueryable()
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                    .ThenInclude(r => r.RentalCopies)
                        .ThenInclude(r => r.BookCopy)
                .SingleOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (subscriberError, _) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(subscriberError))
                return View("NotAllowedRental", subscriberError);

            var (copiesError, newCopies) = await ValidateCopiesAsync(
                subscriber,
                model.SelectedCopies
            );

            if (!string.IsNullOrEmpty(copiesError))
                return View("NotAllowedRental", copiesError);

            var rental = new Rental
            {
                RentalCopies = newCopies,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
            };

            subscriber.Rentals.Add(rental);
            await _unitOfWork.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var rental = await _unitOfWork
                .Rentals.GetQueryable()
                .AsNoTracking()
                .Include(r => r.RentalCopies)
                    .ThenInclude(rc => rc.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .SingleOrDefaultAsync(r => r.Id == id);

            if (rental is null || rental.CreatedOnUtc.Date != DateTime.UtcNow.Date)
                return NotFound();

            var subscriber = await _unitOfWork
                .Subscribers.GetQueryable()
                .AsNoTracking()
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                    .ThenInclude(r => r.RentalCopies)
                .SingleOrDefaultAsync(s => s.Id == rental.SubscriberId);

            var (errorMessage, availableCopiesCount) = ValidateSubscriber(subscriber!, rental.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var currentCopies = rental.RentalCopies.Select(rc => rc.BookCopy);

            var viewModel = new RentalFormViewModel
            {
                Id = rental.Id,
                SubscriberKey = _dataProtector.Protect(subscriber!.Id.ToString()),
                MaxAllowedCopies = availableCopiesCount,
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(currentCopies),
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var rental = await _unitOfWork
                .Rentals.GetQueryable()
                .Include(r => r.RentalCopies)
                    .ThenInclude(rc => rc.BookCopy)
                .SingleOrDefaultAsync(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOnUtc.Date != DateTime.UtcNow.Date)
                return NotFound();

            var subscriber = await _unitOfWork
                .Subscribers.GetQueryable()
                .AsSplitQuery()
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                    .ThenInclude(r => r.RentalCopies)
                        .ThenInclude(rc => rc.BookCopy)
                .SingleOrDefaultAsync(s => s.Id == rental.SubscriberId);

            var (subscriberError, _) = ValidateSubscriber(subscriber!, rental.Id);

            if (!string.IsNullOrEmpty(subscriberError))
                return View("NotAllowedRental", subscriberError);

            var (copiesError, editedCopies) = await ValidateCopiesAsync(
                subscriber!,
                model.SelectedCopies,
                rental.Id
            );

            if (!string.IsNullOrEmpty(copiesError))
                return View("NotAllowedRental", copiesError);

            rental.RentalCopies = editedCopies;
            //_unitOfWork.RentalCopies.Entry(rental).State = EntityState.Modified;

            await _unitOfWork.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        public async Task<IActionResult> Return(int id)
        {
            var rental = await _unitOfWork
                .Rentals.GetQueryable()
                .AsNoTracking()
                .Include(r => r.RentalCopies)
                    .ThenInclude(rc => rc.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .SingleOrDefaultAsync(r => r.Id == id);

            if (rental is null || rental.CreatedOnUtc.Date == DateTime.UtcNow.Date)
                return NotFound();

            var subscriber = await _unitOfWork
                .Subscribers.GetQueryable()
                .AsNoTracking()
                .Include(s => s.Subscriptions)
                .SingleOrDefaultAsync(s => s.Id == rental.SubscriberId);

            var subscriptionEndDate = subscriber!.Subscriptions.Max(sb => sb.EndDate);
            var extendDeadline = rental.StartDate.AddDays(RentalConstants.MaxRentalDuration);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var viewModel = new RentalReturnFormViewModel
            {
                Id = rental.Id,
                RentalCopies = _mapper.Map<IList<RentalCopyViewModel>>(
                    rental.RentalCopies.Where(rc => !rc.ReturnDate.HasValue)
                ),
                SelectedCopies =
                [
                    .. rental
                        .RentalCopies.Where(rc => !rc.ReturnDate.HasValue)
                        .Select(rc => new ReturnCopyViewModel
                        {
                            Id = rc.BookCopyId,
                            IsReturned = rc.ExtendedOn.HasValue ? false : null,
                        }),
                ],
                AllowExtend =
                    !subscriber.IsBlackListed
                    && subscriptionEndDate >= extendDeadline
                    && today <= rental.StartDate.AddDays(RentalConstants.RentalDuration),
            };

            return View("Return", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Return(RentalReturnFormViewModel model)
        {
            var rental = await _unitOfWork
                .Rentals.GetQueryable()
                .Include(r => r.RentalCopies)
                    .ThenInclude(rc => rc.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .SingleOrDefaultAsync(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOnUtc.Date == DateTime.UtcNow.Date)
                return NotFound();

            var copies = _mapper.Map<IList<RentalCopyViewModel>>(
                rental.RentalCopies.Where(rc => !rc.ReturnDate.HasValue)
            );

            var validationResult = _validator.Validate(model);
            validationResult.AddToModelState(ModelState);

            if (!ModelState.IsValid)
            {
                model.RentalCopies = copies;
                return View(model);
            }

            var subscriber = await _unitOfWork
                .Subscribers.GetQueryable()
                .Include(s => s.Subscriptions)
                .SingleOrDefaultAsync(s => s.Id == rental.SubscriberId);

            var subscriptionEndDate = subscriber!.Subscriptions.Max(sb => sb.EndDate);
            var extendDeadline = rental.StartDate.AddDays(RentalConstants.MaxRentalDuration);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (model.SelectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value))
            {
                string? error = null;

                if (subscriber!.IsBlackListed)
                    error = Error.ExtendNotAllowedForBlackListed;
                else if (subscriptionEndDate < extendDeadline)
                    error = Error.ExtendNotAllowedForInactive;
                else if (today > rental.StartDate.AddDays(RentalConstants.RentalDuration))
                    error = Error.ExtendNotAllowed;

                if (!string.IsNullOrEmpty(error))
                {
                    model.RentalCopies = copies;
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }

            var isUpdated = false;

            foreach (var copy in model.SelectedCopies)
            {
                if (!copy.IsReturned.HasValue)
                    continue;

                var currentCopy = rental.RentalCopies.Single(rc => rc.BookCopyId == copy.Id);
                if (currentCopy is null)
                    continue;

                if (copy.IsReturned.HasValue && copy.IsReturned.Value)
                {
                    if (currentCopy.ReturnDate.HasValue)
                        continue;

                    currentCopy.ReturnDate = today;
                    isUpdated = true;
                }

                if (copy.IsReturned.HasValue && !copy.IsReturned.Value)
                {
                    if (currentCopy.ExtendedOn.HasValue)
                        continue;

                    currentCopy.ExtendedOn = today;
                    currentCopy.EndDate = currentCopy.RentalDate.AddDays(
                        RentalConstants.MaxRentalDuration
                    );
                    isUpdated = true;
                }
            }

            if (isUpdated)
            {
                rental.PenaltyPaid = model.PenalityPaid;
                await _unitOfWork.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        [HttpPost]
        public async Task<IActionResult> GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = await _unitOfWork
                .BookCopies.GetQueryable()
                .Include(c => c.Book)
                .SingleOrDefaultAsync(c =>
                    c.SerialNumber.ToString() == model.Value && !c.IsDeleted && !c.Book!.IsDeleted
                );

            if (copy is null)
                return NotFound(Error.InvalidSerialNumber);

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Error.NotAvailableForRental);

            var isInOtherRental = await _unitOfWork
                .RentalCopies.GetQueryable()
                .AnyAsync(rc => rc.BookCopyId == copy.Id && !rc.ReturnDate.HasValue);
            if (isInOtherRental)
                return BadRequest(Error.CopyIsInRental);

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_CopyDetails", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var rental = await _unitOfWork
                .Rentals.GetQueryable()
                .Include(r => r.RentalCopies)
                .SingleOrDefaultAsync(r => r.Id == id);

            if (rental is null || rental.CreatedOnUtc.Date != DateTime.UtcNow.Date)
                return NotFound();

            rental.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();

            return Ok(rental.RentalCopies.Count);
        }

        public async Task<IActionResult> Details(int id)
        {
            var rental = await _unitOfWork
                .Rentals.GetQueryable()
                .AsNoTracking()
                .Include(r => r.RentalCopies)
                    .ThenInclude(rc => rc.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .SingleOrDefaultAsync(r => r.Id == id);

            if (rental is null)
                return NotFound();

            var viewModel = _mapper.Map<RentalViewModel>(rental);
            return View(viewModel);
        }

        private static (string? errorMessage, int? maxAllowedCopies) ValidateSubscriber(
            Subscriber subscriber,
            int? rentalId = null
        )
        {
            if (subscriber.IsBlackListed)
                return (Error.BlackListedSubscriber, null);

            if (
                subscriber.Subscriptions.Max(s => s.EndDate)
                < DateOnly.FromDateTime(
                    DateTime.UtcNow.Date.AddDays((RentalConstants.RentalDuration))
                )
            )
                return (Error.InactiveSubscriber, null);

            var currentRentals = subscriber
                .Rentals.Where(r => rentalId == null || r.Id != rentalId)
                .SelectMany(r => r.RentalCopies)
                .Count(rc => !rc.ReturnDate.HasValue);

            var availableCopiesCount = RentalConstants.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return (Error.MaxAllowedCopiesReached, null);

            return (null, availableCopiesCount);
        }

        private async Task<(
            string? errorMessage,
            ICollection<RentalCopy> rentalCopies
        )> ValidateCopiesAsync(
            Subscriber subscriber,
            IEnumerable<int> subscriberSelectedCopies,
            int? rentalId = null
        )
        {
            var currentSubscriberRentals = subscriber
                .Rentals.SelectMany(r => r.RentalCopies)
                .Where(rc =>
                    !rc.ReturnDate.HasValue && (rentalId == null || rc.RentalId != rentalId)
                )
                .Select(rc => rc.BookCopy!.BookId)
                .ToHashSet();

            var selectedCopies = await _unitOfWork
                .BookCopies.GetQueryable()
                .Include(c => c.Book)
                .Include(c => c.Rentals.Where(r => !r.ReturnDate.HasValue))
                .Where(c => subscriberSelectedCopies.Contains(c.SerialNumber))
                .ToListAsync();

            var existingCopyIds = rentalId.HasValue
                ? await _unitOfWork
                    .RentalCopies.GetQueryable()
                    .Where(rc => rc.RentalId == rentalId)
                    .Select(rc => rc.BookCopyId)
                    .ToHashSetAsync()
                : [];

            var copies = new List<RentalCopy>();

            foreach (var copy in selectedCopies)
            {
                if (existingCopyIds.Contains(copy.Id))
                {
                    copies.Add(new RentalCopy { BookCopyId = copy.Id });
                    continue;
                }

                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                    return (Error.NotAvailableForRental, copies);

                if (copy.Rentals.Any(r => !r.ReturnDate.HasValue))
                    return (Error.CopyIsInRental, copies);

                if (currentSubscriberRentals.Contains(copy.BookId))
                    return ($"This subscriber already has a copy for '{copy.Book.Title}' book", []);

                copies.Add(new RentalCopy { BookCopyId = copy.Id });
            }

            return (null, copies);
        }
    }
}
