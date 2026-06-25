using Bookano.Application.DTOs.Books;
using Bookano.Application.Services.Authors;
using Bookano.Application.Services.BookCopies;
using Bookano.Application.Services.Books;
using Bookano.Application.Services.Categories;
using Bookano.Application.Services.Publishers;
using Bookano.Web.Binders;
using Bookano.Web.ViewModels.Books;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Dynamic.Core;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController(
        IMapper mapper,
        IBookService bookService,
        IBookCopiesService bookCopiesService,
        IAuthorService authorService,
        ICategoryService categoryService,
        IPublisherService publisherService
    ) : Controller
    {
        private readonly IMapper _mapper = mapper;
        private readonly IBookService _bookService = bookService;
        private readonly IBookCopiesService _bookCopiesService = bookCopiesService;
        private readonly IAuthorService _authorService = authorService;
        private readonly ICategoryService _categoryService = categoryService;
        private readonly IPublisherService _publisherService = publisherService;

        public IActionResult Index() => View();

        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetBooks(CancellationToken ct)
        {
            var request = DataTableRequestBinder.Bind(Request.Form);

            var result = await _bookService.GetPagedFilteredAsync<BookListDto>(request, ct);

            return Ok(new
            {
                recordsTotal = result.TotalCount,
                recordsFiltered = result.FilteredCount,
                data = result.Data
            });
        }

        public async Task<IActionResult> Details(int id,CancellationToken ct)
        {
            var book = await _bookService.GetDetailsAsync(id, ct);

            if (book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct) => View("Form", await PopulateViewModelAsync(ct: ct));

        [HttpPost]
        public async Task<IActionResult> Create(BookFormViewModel model,CancellationToken ct)
        {
            var dto = _mapper.Map<BookSaveDto>(model);

            if (model.Image is not null)
            {
                dto.Image = new ImageUploadDto
                {
                    Stream = model.Image.OpenReadStream(),
                    FileName = model.Image.FileName,
                    Length = model.Image.Length,
                };
            }

            var result = await _bookService.CreateAsync(dto, ct);

            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model, ct));

            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var book = await _bookService.GetByIdAsync(id, ct);

            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);
            
            var categories = await _bookService.GetBookCategoriesAsync(id, ct);
            var authors = await _bookService.GetBookAuthorsAsync(id, ct);

            model.SelectedCategories = categories.Select(c => c.Id).ToList();
            model.SelectedAuthors = authors.Select(a => a.Id).ToList();

            var viewModel = await PopulateViewModelAsync(model, ct);

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BookFormViewModel model, CancellationToken ct)
        {
            var dto = _mapper.Map<BookSaveDto>(model);

            if (model.Image is not null)
            {
                dto.Image = new ImageUploadDto
                {
                    Stream = model.Image.OpenReadStream(),
                    FileName = model.Image.FileName,
                    Length = model.Image.Length,
                };
            }

            var result = await _bookService.UpdateAsync(dto.Id,dto, ct);

            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model, ct));

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
        {
            var result = await _bookService.ToggleStatusAsync(id, ct);

            if (result.IsFailure)
                return NotFound();

            return Ok(result.Value!.LastUpdatedOnUtc.ToString());
        }

        public async Task<IActionResult> AllowItem(BookFormViewModel model, CancellationToken ct)
        {
            var isAllowed = await _bookService.IsIsbnAvailableAsync(model.Isbn ?? string.Empty, model.Id, ct);

            return Json(isAllowed);
        }

       
        private async Task<BookFormViewModel> PopulateViewModelAsync(
            BookFormViewModel? model = null,CancellationToken ct = default
        )
        {
            var viewModel = model ?? new BookFormViewModel();

            var authors = await _authorService.GetActiveAsync(ct);

            var categories = await _categoryService.GetActiveAsync(ct);

            var publishers = await _publisherService.GetActiveAsync(ct);

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
            viewModel.Publishers = _mapper.Map<IEnumerable<SelectListItem>>(publishers);

            return viewModel;
        }

    }
}
