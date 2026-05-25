using HashidsNet;

namespace Bookano.Web.Controllers
{
    public class SearchController(IApplicationDbContext context, IHashids hashids, IMapper mapper)
        : Controller
    {
        private readonly IApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IHashids _hashids = hashids;

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Find(string query)
        {
            query = query.Trim();

            var books = await _context
                .Books.AsNoTracking()
                .Include(b => b.Authors)
                    .ThenInclude(a => a.Author)
                .Where(b =>
                    !b.IsDeleted
                    && (
                        b.Title.Contains(query)
                        || b.Authors.Any(a => a.Author!.Name.Contains(query))
                        || (b.Isbn != null && b.Isbn.Contains(query))
                    )
                )
                .Select(b => new
                {
                    Key = _hashids.EncodeHex(b.Id.ToString()),
                    b.Title,
                    Authors = string.Join(", ", b.Authors.Select(a => a.Author!.Name)),
                })
                .ToListAsync();

            return Ok(books);
        }

        public async Task<IActionResult> BookDetails(string bookKey)
        {
            var bookId = _hashids.DecodeHex(bookKey);

            if (bookId.Length == 0)
                return NotFound();

            var book = await _context
                .Books.AsNoTracking()
                .Include(b => b.Copies)
                .Include(b => b.Publisher)
                .Include(b => b.Authors)
                    .ThenInclude(a => a.Author)
                .Include(b => b.Categories)
                    .ThenInclude(c => c.Category)
                .SingleOrDefaultAsync(b => !b.IsDeleted && b.Id == int.Parse(bookId));

            if (book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);

            return View(viewModel);
        }
    }
}
