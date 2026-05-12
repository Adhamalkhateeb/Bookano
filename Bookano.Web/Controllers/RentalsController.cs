using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.DataProtection;

namespace Bookano.Web.Controllers
{
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;

        public RentalsController(
            ApplicationDbContext context,
            IMapper mapper,
            IDataProtectionProvider dataProtector
        )
        {
            _context = context;
            _mapper = mapper;
            _dataProtector = dataProtector.CreateProtector("security");
        }

        public async Task<IActionResult> Create(string subscriberKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(subscriberKey));

            var subscriber = await _context
                .Subscribers.AsNoTracking()
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
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = await _context
                .BookCopies.Include(c => c.Book)
                .SingleOrDefaultAsync(c =>
                    c.SerialNumber.ToString() == model.Value && !c.IsDeleted && !c.Book!.IsDeleted
                );

            if (copy is null)
                return NotFound(Error.InvalidSerialNumber);

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Error.NotAvailableForRental);

            var isInOtherRental = await _context.RentalCopies.AnyAsync(rc =>
                rc.BookCopyId == copy.Id && !rc.ReturnDate.HasValue
            );
            if (isInOtherRental)
                return BadRequest(Error.CopyIsInRental);

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_CopyDetails", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var subscriber = await _context
                .Subscribers.Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                    .ThenInclude(r => r.RentalCopies)
                        .ThenInclude(r => r.BookCopy)
                .SingleOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, _) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var currentSubscriberRentals = subscriber
                .Rentals.SelectMany(r => r.RentalCopies)
                .Where(rc => !rc.ReturnDate.HasValue)
                .Select(rc => rc.BookCopy!.BookId)
                .ToHashSet();

            var selectedCopies = await _context
                .BookCopies.Include(c => c.Book)
                .Include(c => c.Rentals.Where(r => !r.ReturnDate.HasValue))
                .Where(c => model.SelectedCopies.Contains(c.SerialNumber))
                .ToListAsync();

            var newCopies = new List<RentalCopy>();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                    return View("NotAllowedRental", Error.NotAvailableForRental);

                if (copy.Rentals.Any())
                    return View("NotAllowedRental", Error.CopyIsInRental);

                if (currentSubscriberRentals.Contains(copy.BookId))
                    return View(
                        "NotAllowedRental",
                        $"This subscriber already has a copy for '{copy.Book.Title}' book"
                    );

                newCopies.Add(new RentalCopy { BookCopyId = copy.Id });
            }

            var rental = new Rental
            {
                RentalCopies = newCopies,
                CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier),
            };

            subscriber.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private (string? errorMessage, int? maxAllowedCopies) ValidateSubscriber(
            Subscriber subscriber
        )
        {
            if (subscriber.IsBlackListed)
                return (Error.BlackListedSubscriber, null);

            if (
                subscriber.Subscriptions.Max(s => s.EndDate)
                < DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration)
            )
                return (Error.InactiveSubscriber, null);

            var currentRentals = subscriber
                .Rentals.SelectMany(r => r.RentalCopies)
                .Count(rc => !rc.ReturnDate.HasValue);
            var availableCopiesCount = (int)RentalsConfigurations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return (Error.MaxAllowedCopiesReached, null);

            return (null, availableCopiesCount);
        }
    }
}
