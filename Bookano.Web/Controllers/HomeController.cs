using System.Diagnostics;
using Bookano.Web.Core.ViewModels;
using HashidsNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Bookano.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHashids _hashids;

        public HomeController(ApplicationDbContext context, IHashids hashids)
        {
            _context = context;
            _hashids = hashids;
        }

        public async Task<IActionResult> Index()
        {
            //throw new Exception("Error message");

            if (User is not null && User.Identity!.IsAuthenticated)
                return RedirectToAction(nameof(Index), "Dashboard");

            var recentlyAddedBooks = await _context
                .Books.AsNoTracking()
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.CreatedOnUtc)
                .Take(10)
                .Select(b => new BookViewModel
                {
                    Id = b.Id,
                    key = _hashids.EncodeHex(b.Id.ToString()),
                    Title = b.Title,
                    ImageUrl = b.ImageUrl,
                    Authors = b.Authors.Select(a => a.Author!.Name).ToList(),
                })
                .ToListAsync();

            return View(recentlyAddedBooks);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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
