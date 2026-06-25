using Bookano.Application.Services.Books;
using Bookano.Application.Services.Rentals;
using Bookano.Application.Services.Subscribers;
using Bookano.Web.ViewModels.Books;
using Bookano.Web.ViewModels.Dashboard;

namespace Bookano.Web.Controllers
{
    [Authorize]
    public class DashboardController(
        IBookService bookService,
        ISubscriberService subscriberService,
        IRentalService rentalService,
        IMapper mapper) : Controller
    {
        private readonly IBookService _bookService = bookService;
        private readonly ISubscriberService _subscriberService = subscriberService;
        private readonly IRentalService _rentalService = rentalService;
        private readonly IMapper _mapper = mapper;

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var copiesCount = await _rentalService.GetTotalRentedCopiesAsync(cancellationToken);
            var subscribersCount = await _subscriberService.GetActiveSubscribersCountAsync(cancellationToken);
            var recentlyAddedBooks = await _bookService.GetRecentBooksAsync(8,cancellationToken);
            var topRentedBooks = await _bookService.GetTopRentedBooksAsync(6,cancellationToken);

            var recentlyAddedViewModels = _mapper.Map<List<BookViewModel>>(recentlyAddedBooks);
            var topRentedViewModels = _mapper.Map<List<BookViewModel>>(topRentedBooks);

            foreach (var vm in recentlyAddedViewModels)
            {
                var authors = await _bookService.GetBookAuthorsAsync(vm.Id);
                vm.Authors = authors.Select(a => a.Name).ToList();
            }

            foreach (var vm in topRentedViewModels)
            {
                var authors = await _bookService.GetBookAuthorsAsync(vm.Id);
                vm.Authors = authors.Select(a => a.Name).ToList();
            }

            var viewModel = new DashboardViewModel
            {
                NumberOfCopies = copiesCount,
                NumberOfSubscribers = subscribersCount,
                RecentlyAddedBooks = recentlyAddedViewModels,
                TopRentedBooks = topRentedViewModels
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetRentalsPerDay(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null
        )
        {
            var start = startDate is not null
                ? DateOnly.ParseExact(startDate, "yyyy-MM-dd")
                : (DateOnly?)null;

            var end = endDate is not null
                ? DateOnly.ParseExact(endDate, "yyyy-MM-dd")
                : (DateOnly?)null;

            var data = await _rentalService.GetRentalsPerDayAsync(start, end);
            var figures = _mapper.Map<IEnumerable<ChartItemViewModel>>(data);

            return Ok(figures);
        }

        public async Task<IActionResult> GetSubscribersPerGovernorate()
        {
            var data = await _subscriberService.GetSubscribersPerGovernorateAsync();
            var result = _mapper.Map<IEnumerable<ChartItemViewModel>>(data);

            return Ok(result);
        }
    }
}
