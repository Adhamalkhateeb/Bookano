using Bookano.Application.DTOs.Authors;
using Bookano.Application.Services.Authors;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class AuthorsController(
        IMapper mapper,
        IValidator<AuthorFormViewModel> validator,
        IAuthorService authorService
    ) : Controller
    {
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<AuthorFormViewModel> _validator = validator;
        private readonly IAuthorService _authorService = authorService;

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var authors = await _authorService.GetAllAsync(ct);

            var viewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);

            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        public async Task<IActionResult> Create(AuthorFormViewModel model, CancellationToken ct)
        {
            var validation = _validator.Validate(model);
            validation.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            var dto = _mapper.Map<AuthorFormDto>(model);

            var result = await _authorService.AddAsync(dto, ct);

            var vm = _mapper.Map<AuthorViewModel>(result);

            return PartialView("_AuthorRow", vm);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var author = await _authorService.GetAsync(id, ct);

            if (author is null)
                return NotFound();

            var vm = _mapper.Map<AuthorFormViewModel>(author);

            return PartialView("_Form", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AuthorFormViewModel model, CancellationToken ct)
        {
            var validationResult = _validator.Validate(model);
            validationResult.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            var dto = _mapper.Map<AuthorFormDto>(model);

            var result = await _authorService.UpdateAsync(model.Id, dto, ct);

            if (result is null)
                return NotFound();

            var vm = _mapper.Map<AuthorViewModel>(result);

            return PartialView("_AuthorRow", vm);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
        {
            var result = await _authorService.ToggleAsync(id, ct);

            if (result is null)
                return NotFound();

            return Ok(result.LastUpdatedOnUtc.ToString());
        }

        public async Task<IActionResult> AllowItem(AuthorFormViewModel model, CancellationToken ct)
        {
            var isAllowed = await _authorService.IsNameAvailableAsync(model.Id, model.Name, ct);

            return Json(isAllowed);
        }
    }
}
