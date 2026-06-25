using Bookano.Application.Services.Books;
using Bookano.Web.ViewModels.Books;
using HashidsNet;

namespace Bookano.Web.Controllers
{
    public class SearchController(IBookService bookService, IHashids hashids, IMapper mapper)
        : Controller
    {
        private readonly IBookService _bookService = bookService;
        private readonly IHashids _hashids = hashids;
        private readonly IMapper _mapper = mapper;

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Find(string query)
        {
            var data = await _bookService.GetFilteredBooksAsync(query);

            var books = new List<object>();
            foreach(var b in data)
            {
                var authors = await _bookService.GetBookAuthorsAsync(b.Id);
                books.Add(new
                {
                    Key = _hashids.EncodeHex(b.Id.ToString()),
                    b.Title,
                    Authors = string.Join(", ", authors.Select(a => a.Name))
                });
            }

            return Ok(books);
        }

        public async Task<IActionResult> BookDetails(string bookKey, CancellationToken ct)
        {
            var bookIdStr = _hashids.DecodeHex(bookKey);

            if (bookIdStr.Length == 0 || !int.TryParse(bookIdStr, out var bookId))
                return NotFound();

            var book = await _bookService.GetByIdAsync(bookId, ct);

            if (book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);
            var categories = await _bookService.GetBookCategoriesAsync(bookId, ct);
            var authors = await _bookService.GetBookAuthorsAsync(bookId, ct);

            viewModel.Categories = categories.Select(c => c.Name).ToList();
            viewModel.Authors = authors.Select(a => a.Name).ToList();

            return View(viewModel);
        }
    }
}
