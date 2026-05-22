using System.Linq.Dynamic.Core;
using Bookano.Web.Services.Image;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public BooksController(
            ApplicationDbContext context,
            IMapper mapper,
            [FromKeyedServices("cloudinary")] IImageService imageService
        )
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        public IActionResult Index() => View();

        [HttpPost, IgnoreAntiforgeryToken]
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
        public async Task<IActionResult> Create() => View("Form", await PopulateViewModelAsync());

        [HttpPost]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            if (model.Image is not null)
            {
                var imageValidationError = _imageService.ValidateImage(model.Image);
                if (imageValidationError is not null)
                {
                    ModelState.AddModelError("Image", imageValidationError);
                    return View("Form", await PopulateViewModelAsync(model));
                }
            }

            var duplicate = await _context.Books.FirstOrDefaultAsync(b =>
                b.IdempotencyKey == model.IdempotencyKey
            );

            if (duplicate is not null)
                return RedirectToAction(nameof(Details), new { duplicate.Id });

            var book = _mapper.Map<Book>(model);
            book.IdempotencyKey = model.IdempotencyKey;

            SyncCategories(book, model.SelectedCategories);
            SyncAuthors(book, model.SelectedAuthors);
            book.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.Books.Add(book);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var existing = await _context.Books.FirstAsync(b =>
                    b.IdempotencyKey == model.IdempotencyKey
                );
                return RedirectToAction(nameof(Details), new { existing.Id });
            }

            if (model.Image is not null)
            {
                var uploadResult = await _imageService.UploadAsync(model.Image, "books", null);

                if (uploadResult.IsSuccess)
                {
                    ApplyImage(book, uploadResult.Url!, uploadResult.PublicId!);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["Message"] = uploadResult.ErrorMessage;
                }
            }

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
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var book = await _context
                .Books.Include(b => b.Categories)
                .Include(b => b.Authors)
                .Include(b => b.Copies)
                .SingleOrDefaultAsync(b => b.Id == model.Id);

            if (book is null)
                return NotFound();

            if (model.Image is not null)
            {
                var imageValidationError = _imageService.ValidateImage(model.Image);
                if (imageValidationError is not null)
                {
                    ModelState.AddModelError("Image", imageValidationError);
                    return View("Form", await PopulateViewModelAsync(model));
                }
            }

            var dbImagePublicId = book.ImagePublicId;
            var shouldUpload = model.Image is not null;
            var shouldRemove = model.RemoveImage && !string.IsNullOrEmpty(dbImagePublicId);

            book = _mapper.Map(model, book);
            book.ImagePublicId = dbImagePublicId;

            SyncCategories(book, model.SelectedCategories);
            SyncAuthors(book, model.SelectedAuthors);
            book.LastUpdatedOnUtc = DateTime.UtcNow;
            book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (!book.IsAvailableForRental)
                foreach (var copy in book.Copies)
                    copy.IsAvailableForRental = false;

            if (model.RowVersion is not null)
                _context.Entry(book).Property(b => b.RowVersion).OriginalValue = model.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction(nameof(Details), new { model.Id });
            }

            if (shouldUpload)
            {
                var uploadResult = await _imageService.UploadAsync(model.Image!, "books", null);

                if (uploadResult.IsSuccess)
                {
                    var oldImagePublicId = dbImagePublicId;

                    ApplyImage(book, uploadResult.Url!, uploadResult.PublicId!);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(oldImagePublicId))
                    {
                        await _imageService.DeleteAsync(oldImagePublicId);
                    }
                }
                else
                {
                    TempData["Message"] = uploadResult.ErrorMessage;
                }
            }
            else if (shouldRemove)
            {
                var deleteResult = await _imageService.DeleteAsync(dbImagePublicId!);

                if (!deleteResult.IsSuccess)
                {
                    TempData["Message"] = deleteResult.ErrorMessage;
                }
                else
                {
                    book.ImageUrl = null;
                    book.ImageThumbnailUrl = null;
                    book.ImagePublicId = null;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Details), new { book.Id });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book is null)
                return NotFound();

            book.IsDeleted = !book.IsDeleted;
            var updatedOn = DateTimeOffset.UtcNow;
            book.LastUpdatedOnUtc = updatedOn;
            book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            await _context.SaveChangesAsync();

            return Ok(updatedOn.ToString("o"));
        }

        public async Task<IActionResult> AllowItem(BookFormViewModel model)
        {
            var book = await _context.Books.SingleOrDefaultAsync(b => b.Isbn == model.Isbn);
            var isAllowed = book is null || book.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        private void ApplyImage(Book book, string url, string publicId)
        {
            book.ImageUrl = url;
            book.ImagePublicId = publicId;
            book.ImageThumbnailUrl = _imageService.GetThumbnail(publicId);
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
    }
}
