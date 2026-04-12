using Microsoft.AspNetCore.Mvc;


namespace Bookano.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // TODO: Using ViewModels instead of passing entities directly to the view
            var categories = _context.Categories.AsNoTracking().ToList();
            return View(categories);
        }
    }
}
