using Bookano.Application.Services.Search;
using Bookano.Web.ViewModels.Books;
using HashidsNet;

namespace Bookano.Web.Controllers
{
    public class SearchController(ISearchService searchService, IHashids hashids, IMapper mapper)
        : Controller
    {
        private readonly ISearchService _searchService = searchService;
        private readonly IHashids _hashids = hashids;
        private readonly IMapper _mapper = mapper;

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Find(string query)
        {
            var data = await _searchService.FindBooksAsync(query);

            var books = data.Select(b => new
            {
                Key = _hashids.EncodeHex(b.Id.ToString()),
                b.Title,
                b.Authors,
            });

            return Ok(books);
        }

        public async Task<IActionResult> BookDetails(string bookKey, CancellationToken ct)
        {
            var bookIdStr = _hashids.DecodeHex(bookKey);

            if (bookIdStr.Length == 0 || !int.TryParse(bookIdStr, out var bookId))
                return NotFound();

            var bookDto = await _searchService.GetBookDetailsAsync(bookId, ct);

            if (bookDto is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(bookDto);

            return View(viewModel);
        }
    }
}
