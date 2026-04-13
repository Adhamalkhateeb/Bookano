using Bookano.Web.Core.Models;
using Bookano.Web.Filters;
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

        [HttpGet]
        public IActionResult Index()
        {
            // TODO: Using ViewModels instead of passing entities directly to the view
            var categories = _context.Categories.AsNoTracking().ToList();
            return View(categories);
        }


        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = new Category { Name = model.Name };

            _context.Categories.Add(category);
            _context.SaveChanges();

            return PartialView("_CategoryRow",category);
        }


        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);

            if (category is null)
                return NotFound();

            var categoryViewModel = new CategoryFormViewModel
            {
                Id = id,
                Name = category.Name
            };

            return PartialView("_Form",categoryViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _context.Categories.Find(model.Id);

            if (category is null)
                return NotFound();

            category.Name = model.Name;
            category.LastUpdatedOnUtc = DateTime.UtcNow;
            _context.SaveChanges();

            return PartialView("_CategoryRow", category);

        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            
            var category = _context.Categories.Find(id);

            if (category is null)
                return NotFound();

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOnUtc = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                isDeleted = category.IsDeleted,
                lastUpdatedOn = category.LastUpdatedOnUtc.GetValueOrDefault().ToLocalTime().ToString("yyyy/MM/dd hh:mm tt")
            });
        }
    }
}
