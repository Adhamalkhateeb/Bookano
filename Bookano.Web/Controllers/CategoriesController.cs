namespace Bookano.Web.Controllers;

[Authorize(Roles = AppRoles.Archive)]
public class CategoriesController(IApplicationDbContext context, IMapper mapper) : Controller
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.AsNoTracking().ToListAsync();

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
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var category = _mapper.Map<Category>(model);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var categoryViewModel = _mapper.Map<CategoryViewModel>(category);

        return PartialView("_CategoryRow", categoryViewModel);
    }

    [HttpGet]
    [AjaxOnly]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category is null)
            return NotFound();

        var categoryFormViewModel = _mapper.Map<CategoryFormViewModel>(category);

        return PartialView("_Form", categoryFormViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var category = await _context.Categories.FindAsync(model.Id);

        if (category is null)
            return NotFound();

        category = _mapper.Map(model, category);

        await _context.SaveChangesAsync();

        var categoryViewModel = _mapper.Map<CategoryViewModel>(category);

        return PartialView("_CategoryRow", categoryViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category is null)
            return NotFound();

        category.IsDeleted = !category.IsDeleted;

        await _context.SaveChangesAsync();

        return Ok(category.LastUpdatedOnUtc.ToString());
    }

    public async Task<IActionResult> AllowItem(CategoryFormViewModel model)
    {
        var category = await _context.Categories.SingleOrDefaultAsync(c => c.Name == model.Name);
        var isAllowed = category is null || category.Id.Equals(model.Id);

        return Json(isAllowed);
    }
}
