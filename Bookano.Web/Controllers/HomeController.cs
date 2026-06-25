using Bookano.Application.Services.Books;
using Bookano.Web.ViewModels;
using Bookano.Web.ViewModels.Books;
using HashidsNet;
using Microsoft.AspNetCore.WebUtilities;

namespace Bookano.Web.Controllers
{
    public class HomeController(IBookService bookService, IMapper mapper, IHashids hashids) : Controller
    {
        private readonly IBookService _bookService = bookService;
        private readonly IMapper _mapper = mapper;
        private readonly IHashids _hashids = hashids;

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            if (User is not null && User.Identity!.IsAuthenticated)
                return RedirectToAction(nameof(Index), "Dashboard");

            var recentlyAddedBooks = await _bookService.GetRecentBooksAsync(6,cancellationToken);

            var viewModel = _mapper.Map<List<BookViewModel>>(recentlyAddedBooks);

            foreach (var vm in viewModel)
            {
                vm.Key = _hashids.EncodeHex(vm.Id.ToString());
                var authors = await _bookService.GetBookAuthorsAsync(vm.Id);
                vm.Authors = authors.Select(a => a.Name).ToList();
            }

            return View(viewModel);
        }

        public IActionResult Error(int statusCode = 500)
        {
            return View(
                new ErrorViewModel
                {
                    ErrorCode = statusCode,
                    ErrorMessage = ReasonPhrases.GetReasonPhrase(statusCode),
                }
            );
        }
    }
}
