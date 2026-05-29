using Bookano.Application.Common.Models;
using Bookano.Application.DTOs.Books;
using Bookano.Application.Interfaces;
using Bookano.Application.Services.Authors;
using Bookano.Application.Services.Books;
using Bookano.Application.Services.Categories;
using Bookano.Application.Services.Publishers;
using Bookano.Web.Binders;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Dynamic.Core;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController(
        IMapper mapper,
        IBookService bookService,
        IAuthorService authorService,
        ICategoryService categoryService,
        IPublisherService publisherService
    ) : Controller
    {
        private readonly IMapper _mapper = mapper;
        private readonly IBookService _bookService = bookService;
        private readonly IAuthorService _authorService = authorService;
        private readonly ICategoryService _categoryService = categoryService;
        private readonly IPublisherService _publisherService = publisherService;

        public IActionResult Index() => View();

        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetBooks(CancellationToken ct)
        {
            var request = DataTableRequestBinder.Bind(Request.Form);

            var data = await _bookService.GetPagedAsync(request, ct);

            return Ok(data);
        }

        public async Task<IActionResult> Details(int id,CancellationToken ct)
        {
            var book = await _bookService.GetBookDetailsAsync(id);

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
            var dto = _mapper.Map<BookFormDto>(model);

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
            var book = await _bookService.GetBookFormAsync(id, ct);

            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = await PopulateViewModelAsync(model, ct);

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BookFormViewModel model, CancellationToken ct)
        {
            var dto = _mapper.Map<BookFormDto>(model);

            if (model.Image is not null)
            {
                dto.Image = new ImageUploadDto
                {
                    Stream = model.Image.OpenReadStream(),
                    FileName = model.Image.FileName,
                    Length = model.Image.Length,
                };
            }

            var result = await _bookService.UpdateAsync(dto, ct);

            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model, ct));

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
        {
            var lastUpdatedOnUtc = await _bookService.ToggleAsync(id, ct);

            if (lastUpdatedOnUtc is null)
                return NotFound();

            return Ok(lastUpdatedOnUtc.Value.ToString());
        }

        public async Task<IActionResult> AllowItem(BookFormViewModel model, CancellationToken ct)
        {
            var isAllowed = await _bookService.IsIsbnUniqueAsync(model.Isbn ?? string.Empty, model.Id, ct);

            return Json(isAllowed);
        }

       
        private async Task<BookFormViewModel> PopulateViewModelAsync(
            BookFormViewModel? model = null,CancellationToken ct = default
        )
        {
            var viewModel = model ?? new BookFormViewModel();

            var authors = await _authorService.GetAllActiveAsync(ct);

            var categories = await _categoryService.GetAllActiveAsync(ct);

            var publishers = await _publisherService.GetAllActiveAsync(ct);

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
            viewModel.Publishers = _mapper.Map<IEnumerable<SelectListItem>>(publishers);

            return viewModel;
        }

    }
}
