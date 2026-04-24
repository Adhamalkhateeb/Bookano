using System.Linq.Dynamic.Core;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [HttpPost]
        public async Task<IActionResult> GetBooks()
        {
            int skip = int.TryParse(Request.Form["start"], out var parsedSkip) ? parsedSkip : 0;
            int pageSize =
                int.TryParse(Request.Form["length"], out var parsedPageSize) && parsedPageSize > 0
                    ? parsedPageSize
                    : 10;
            var searchValue = Request.Form["search[value]"].ToString();

            var sortColumnIndex = int.TryParse(
                Request.Form["order[0][column]"],
                out var parsedSortColumnIndex
            )
                ? parsedSortColumnIndex
                : 0;

            var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Id",
                "Title",
                "Publisher.Name",
                "PublishingDate",
                "Hall",
                "IsAvailableForRental",
                "IsDeleted",
            };

            var requestedColumn = Request.Form[$"columns[{sortColumnIndex}][name]"].ToString();
            var sortColumn = allowedSortColumns.Contains(requestedColumn) ? requestedColumn : "Id";

            var isDescending = string.Equals(
                Request.Form["order[0][dir]"].ToString(),
                "desc",
                StringComparison.OrdinalIgnoreCase
            );
            var sortDirection = isDescending ? "desc" : "asc";

            var booksQuery = _context.Books.AsNoTracking().AsQueryable();

            var totalRecords = await booksQuery.CountAsync();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(searchValue)
                    || (b.Isbn != null && b.Isbn.Contains(searchValue))
                    || b.Authors.Any(a => a.Author != null && a.Author.Name.Contains(searchValue))
                );
            }

            booksQuery = booksQuery.OrderBy($"{sortColumn} {sortDirection}");

            var filteredRecords = await booksQuery.CountAsync();

            var result = await booksQuery
                .Include(b => b.Publisher)
                .Include(b => b.Authors)
                    .ThenInclude(a => a.Author)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<BookViewModel>>(result);

            return Ok(
                new
                {
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = mappedData,
                }
            );
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context
                .Books.AsNoTracking()
                .Include(b => b.Copies)
                .Include(b => b.Publisher)
                .Include(b => b.Authors)
                    .ThenInclude(a => a.Author)
                .Include(b => b.Categories)
                    .ThenInclude(c => c.Category)
                .SingleOrDefaultAsync(b => b.Id == id);

            if (book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);

            return View(viewModel);
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

            return RedirectToAction(nameof(Details), new { book.Id });
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
            else if (model.RemoveImage)
            {
                if (!string.IsNullOrEmpty(book.ImagePublicId))
                    await DeleteImageAsync(book.ImagePublicId);

                book.ImageUrl = null;
                book.ImageThumbnailUrl = null;
                book.ImagePublicId = null;
            }

            SyncCategories(book, model.SelectedCategories);
            SyncAuthors(book, model.SelectedAuthors);
            book.LastUpdatedOnUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { book.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book is null)
                return NotFound();

            book.IsDeleted = !book.IsDeleted;
            var updatedOn = DateTimeOffset.UtcNow;
            book.LastUpdatedOnUtc = updatedOn;

            await _context.SaveChangesAsync();

            return Ok(updatedOn.ToString("o"));
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
                    new Transformation()
                        .Width(125)
                        .Crop("scale")
                        .Quality("auto:good")
                        .FetchFormat("auto")
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
