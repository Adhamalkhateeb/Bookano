using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using System.Collections.Specialized;

namespace Bookano.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DashboardController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var copiesCount = await _context.BookCopies.CountAsync(c => !c.IsDeleted);
            copiesCount = copiesCount <= 10 ? copiesCount : copiesCount / 10 * 10;

            var subscribersCount = await _context.Subscribers.CountAsync(s => !s.IsDeleted);

            var recentlyAddedBooks = await _context.Books
                 .AsNoTracking()
                 .Where(b => !b.IsDeleted)
                 .OrderByDescending(b => b.CreatedOnUtc)
                 .Take(8)
                 .Select(b => new BookViewModel
                 {
                     Id = b.Id,
                     Title = b.Title,
                     ImageUrl = b.ImageUrl,
                     Authors = b.Authors.Select(a => a.Author!.Name).ToList()
                 })
                 .ToListAsync();


            var topBookIds = await _context.RentalCopies
                .GroupBy(rc => rc.BookCopy!.BookId)
                .OrderByDescending(g => g.Count())
                .Take(6)
                .Select(g => g.Key)
                .ToListAsync();

            var topRentedBooks = await _context.Books
                .AsNoTracking()
                .Where(b => topBookIds.Contains(b.Id) && !b.IsDeleted)
                .Include(b => b.Authors)
                    .ThenInclude(ba => ba.Author)
                .Select(b => new BookViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    ImageUrl = b.ImageUrl,
                    Authors = b.Authors
                        .Select(a => a.Author!.Name)
                        .ToList()
                })
                .ToListAsync();

            var orderedBooks = topBookIds
                .Join(
                    topRentedBooks,
                    id => id,
                    book => book.Id,
                    (id, book) => book
                )
                .ToList();


            var viewModel = new DashboardViewModel
            {
                NumberOfCopies = copiesCount,
                NumberOfSubscribers = subscribersCount,
                RecentlyAddedBooks = recentlyAddedBooks,
                TopRentedBooks = orderedBooks
            };
            return View(viewModel);
        }



        [HttpGet]
        public async Task<IActionResult> GetRentalsPerDay(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null)
        {
            var start = startDate is not null
                ? DateOnly.ParseExact(startDate, "yyyy-MM-dd")
                : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-29));

            var end = endDate is not null
                ? DateOnly.ParseExact(endDate, "yyyy-MM-dd")
                : DateOnly.FromDateTime(DateTime.UtcNow);

            var data = await _context.RentalCopies
                .Where(rc => rc.RentalDate >= start && rc.RentalDate <= end)
                .GroupBy(rc => rc.RentalDate)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var figures = new List<ChartItemViewModel>();

            for (var day = start; day <= end; day = day.AddDays(1))
            {
                var count = data.FirstOrDefault(d => d.Date == day)?.Count ?? 0;
                figures.Add(new ChartItemViewModel
                {
                    Label = day.ToString("d MMM"),
                    Value = count.ToString()
                });
            }

            return Ok(figures);
        }

        public async Task<IActionResult> GetSubscribersPerGovernorate()
        {
            var data = await _context.Subscribers
                .Where(s => !s.IsDeleted)
                .GroupBy(s => new { GovernorateName = s.Governorate!.Name })
                .Select(g => new ChartItemViewModel
                {
                    Label = g.Key.GovernorateName,
                    Value = g.Count().ToString()
                }).ToListAsync();

            return Ok(data);
        }
    }
    }
