using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ReportsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Books(int? pageNumber)
        {
            var authors = await _context.Authors.OrderBy(a => a.Name).AsNoTracking().ToListAsync();
            var categoris = await _context
                .Categories.OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            IQueryable<Book> books = _context
                .Books.AsNoTracking()
                .Include(b => b.Authors)
                    .ThenInclude(a => a.Author)
                .Include(b => b.Categories)
                    .ThenInclude(c => c.Category);

            var viewModel = new BooksReportViewModel
            {
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors),
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categoris),
            };

            if (pageNumber.HasValue)
                viewModel.Books = await PaginatedList<Book>.CreateAsync(
                    books,
                    pageNumber.Value,
                    (int)ReportsConfigurations.PageSize
                );

            return View(viewModel);
        }
    }
}
