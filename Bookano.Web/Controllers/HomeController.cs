using Bookano.Application.Services.Home;
using Bookano.Web.ViewModels;
using Bookano.Web.ViewModels.Books;
using HashidsNet;
using Microsoft.AspNetCore.WebUtilities;

namespace Bookano.Web.Controllers
{
    public class HomeController(IHomeService homeService, IMapper mapper, IHashids hashids) : Controller
    {
        private readonly IHomeService _homeService = homeService;
        private readonly IMapper _mapper = mapper;
        private readonly IHashids _hashids = hashids;

        public async Task<IActionResult> Index()
        {
            if (User is not null && User.Identity!.IsAuthenticated)
                return RedirectToAction(nameof(Index), "Dashboard");

            var recentlyAddedBooks = await _homeService.GetRecentlyAddedBooksAsync();

            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(recentlyAddedBooks);

            foreach (var vm in viewModel)
            {
                vm.Key = _hashids.EncodeHex(vm.Id.ToString());
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
