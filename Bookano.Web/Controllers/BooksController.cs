using AspNetCoreGeneratedDocument;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;

namespace Bookano.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private const int _maxAllowedSize = 2 * 1024 * 1024;

        public BooksController(
            ApplicationDbContext context,
            IMapper mapper,
            IOptions<CloudinarySettings> options
        )
        {
            _context = context;
            _mapper = mapper;

            var account = new Account(
                options.Value.Cloud,
                options.Value.ApiKey,
                options.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
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
                var upload = await UploadImageAsync(model.Image);

                if (upload.url is null)
                    return View("Form", await PopulateViewModelAsync(model));

                ApplyImage(book, upload.url, upload.publicId!);
            }

            SyncCategories(book, model.SelectedCategories);
            SyncAuthors(book, model.SelectedAuthors);

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
                .Include(b => b.Authors)
                .SingleOrDefaultAsync(b => b.Id == id);

            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = await PopulateViewModelAsync(model);

            viewModel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();
            viewModel.SelectedAuthors = book.Authors.Select(a => a.AuthorId).ToList();

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var book = await _context
                .Books.Include(b => b.Categories)
                .Include(b => b.Authors)
                .SingleOrDefaultAsync(b => b.Id == model.Id);

            if (book is null)
                return NotFound();

            _mapper.Map(model, book);

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImagePublicId))
                    await DeleteImageAsync(book.ImagePublicId);

                var upload = await UploadImageAsync(model.Image);

                if (upload.url is null)
                    return View("Form", await PopulateViewModelAsync(model));

                ApplyImage(book, upload.url, upload.publicId!);
            }

            SyncCategories(book, model.SelectedCategories);
            SyncAuthors(book, model.SelectedAuthors);
            book.LastUpdatedOnUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AllowItem(BookFormViewModel model)
        {
            var book = await _context.Books.SingleOrDefaultAsync(b => b.Isbn == model.Isbn);
            var isAllowed = book is null || book.Id.Equals(model.Id);

            return Json(isAllowed);
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

            var publishers = await _context
                .Publishers.Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
            viewModel.Publishers = _mapper.Map<IEnumerable<SelectListItem>>(publishers);

            return viewModel;
        }

        private void SyncCategories(Book book, IEnumerable<int> selected)
        {
            var set = selected.ToHashSet();

            foreach (var category in book.Categories.ToList())
                if (!set.Contains(category.CategoryId))
                    book.Categories.Remove(category);

            foreach (var id in set)
                if (!book.Categories.Any(c => c.CategoryId == id))
                    book.Categories.Add(new BookCategory { CategoryId = id });
        }

        private void SyncAuthors(Book book, IEnumerable<int> selected)
        {
            var set = selected.ToHashSet();

            foreach (var author in book.Authors.ToList())
                if (!set.Contains(author.AuthorId))
                    book.Authors.Remove(author);

            foreach (var id in set)
                if (!book.Authors.Any(c => c.AuthorId == id))
                    book.Authors.Add(new BookAuthor { AuthorId = id });
        }

        private void ApplyImage(Book book, string url, string publicId)
        {
            book.ImageUrl = url;
            book.ImagePublicId = publicId;

            book.ImageThumbnailUrl = _cloudinary
                .Api.UrlImgUp.Transform(
                    new Transformation().Width(150).Height(220).Crop("fill").Gravity("auto")
                )
                .BuildUrl(publicId);
        }

        private async Task<(string? url, string? publicId)> UploadImageAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!_allowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("Image", Core.Consts.Error.NotAllowedImageExtension);
                return (null, null);
            }

            if (file.Length > _maxAllowedSize)
            {
                ModelState.AddModelError("Image", Core.Consts.Error.ImageMaxSizeLimit);
                return (null, null);
            }

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = "books",
                UseFilename = true,
                UniqueFilename = true,
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            return (result.SecureUrl.ToString(), result.PublicId);
        }

        private async Task DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deletionParams);
        }
    }
}
