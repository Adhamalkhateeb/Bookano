using Microsoft.AspNetCore.Mvc;

namespace Bookano.Web.Controllers
{
    public class AuthorsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
