using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bookano.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        private IList<string> _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        private int _maxAllowedSize = 2097152;

        public BooksController(
            ApplicationDbContext context,
            IMapper mapper,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View("Form", await PopulateViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var book = _mapper.Map<Book>(model);

            if (model.Image is not null)
            {
                var imageUrl = await UploadImageAsync(model.Image);

                if (string.IsNullOrEmpty(imageUrl))
                    return View("Form", await PopulateViewModelAsync(model));

                book.ImageUrl = imageUrl;
            }

            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context
                .Books.AsNoTracking()
                .Include(b => b.Categories)
                .SingleOrDefaultAsync(b => b.Id == id);

            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = await PopulateViewModelAsync(model);

            viewModel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();

            return View("Form", await PopulateViewModelAsync(viewModel));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var book = await _context
                .Books.Include(b => b.Categories)
                .SingleOrDefaultAsync(b => b.Id == model.Id);

            if (book is null)
                return NotFound();

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                    RemoveImage(book.ImageUrl);

                var imageUrl = await UploadImageAsync(model.Image);

                if (string.IsNullOrEmpty(imageUrl))
                    return View("Form", await PopulateViewModelAsync(model));

                model.ImageUrl = imageUrl;
            }
            else
            {
                model.ImageUrl = book.ImageUrl;
            }

            _mapper.Map(model, book);

            var selected = model.SelectedCategories.ToHashSet();

            foreach (var c in book.Categories.ToList())
            {
                if (!selected.Contains(c.CategoryId))
                    book.Categories.Remove(c);
            }

            foreach (var id in selected)
            {
                if (!book.Categories.Any(c => c.CategoryId == id))
                    book.Categories.Add(new BookCategory { CategoryId = id });
            }
            book.LastUpdatedOnUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<BookFormViewModel> PopulateViewModelAsync(
            BookFormViewModel? model = null
        )
        {
            var viewModel = model ?? new BookFormViewModel();

            var authors = await _context
                .Authors.AsNoTracking()
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var categories = await _context
                .Categories.AsNoTracking()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);

            return viewModel;
        }

        private async Task<string?> UploadImageAsync(IFormFile image)
        {
            var imageExtension = Path.GetExtension(image.FileName);
            if (!_allowedExtensions.Contains(imageExtension))
            {
                ModelState.AddModelError("Image", Error.NotAllowedImageExtension);
                return null;
            }

            if (image.Length > _maxAllowedSize)
            {
                ModelState.AddModelError("Image", Error.ImageMaxSizeLimit);
                return null;
            }

            var imageUrl = $"{Guid.NewGuid()}{imageExtension}";
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books", imageUrl);

            using var stream = System.IO.File.Create(path);
            await image.CopyToAsync(stream);
            return imageUrl;
        }

        private void RemoveImage(string imageUrl)
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books", imageUrl);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}
