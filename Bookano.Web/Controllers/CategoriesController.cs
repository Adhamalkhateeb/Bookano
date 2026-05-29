using Bookano.Application.DTOs.Categories;
using Bookano.Application.Interfaces;
using Bookano.Application.Services.Categories;

namespace Bookano.Web.Controllers;

[Authorize(Roles = AppRoles.Archive)]
public class CategoriesController(
    IMapper mapper,
    ICategoryService categoryService
) : Controller
{
    private readonly IMapper _mapper = mapper;
    private readonly ICategoryService _categoryService = categoryService;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var categories = await _categoryService.GetAllAsync(ct);

        var viewModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

        return View(viewModel);
    }

    [HttpGet]
    [AjaxOnly]
    public IActionResult Create()
    {
        return PartialView("_Form");
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken ct = default)
    {
        var dto = _mapper.Map<CategoryFormDto>(model);
        var result = await _categoryService.AddAsync(dto, ct);

        result.AddToModelState(ModelState);

        if (!ModelState.IsValid)
            return BadRequest();

        var categoryViewModel = _mapper.Map<CategoryViewModel>(result.Value);

        return PartialView("_CategoryRow", categoryViewModel);
    }

    [HttpGet]
    [AjaxOnly]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);

        if (category is null)
            return NotFound();

        var categoryFormViewModel = _mapper.Map<CategoryFormViewModel>(category);

        return PartialView("_Form", categoryFormViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CategoryFormViewModel model, CancellationToken ct = default)
    {
        var dto = _mapper.Map<CategoryFormDto>(model);
        var result = await _categoryService.UpdateAsync(model.Id, dto, ct);

        result.AddToModelState(ModelState);

        if (!ModelState.IsValid)
            return BadRequest();

        var categoryViewModel = _mapper.Map<CategoryViewModel>(result.Value);

        return PartialView("_CategoryRow", categoryViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct = default)
    {
        var lastUpdatedOnUtc = await _categoryService.ToggleAsync(id, ct);

        if (!lastUpdatedOnUtc.HasValue)
            return NotFound();

        return Ok(lastUpdatedOnUtc.Value.ToString());
    }

    public async Task<IActionResult> AllowItem(CategoryFormViewModel model, CancellationToken ct = default)
    {
        var dto = _mapper.Map<CategoryFormDto>(model);
        var isAllowed = await _categoryService.IsCategoryAllowedAsync(dto, ct);

        return Json(isAllowed);
    }
}
