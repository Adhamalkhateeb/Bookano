namespace Bookano.Web.Controllers;

public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CategoriesController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

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
    [ValidateAntiForgeryToken]
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var category = await _context.Categories.FindAsync(model.Id);

        if (category is null)
            return NotFound();

        category = _mapper.Map(model, category);
        category.LastUpdatedOnUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var categoryViewModel = _mapper.Map<CategoryViewModel>(category);

        return PartialView("_CategoryRow", categoryViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category is null)
            return NotFound();

        category.IsDeleted = !category.IsDeleted;
        var updatedOn = DateTimeOffset.UtcNow;
        category.LastUpdatedOnUtc = updatedOn;

        await _context.SaveChangesAsync();

        return Ok(updatedOn.ToString("o"));
    }

    public async Task<IActionResult> AllowItem(CategoryFormViewModel model)
    {
        var category = await _context.Categories.SingleOrDefaultAsync(c => c.Name == model.Name);
        var isAllowed = category is null || category.Id.Equals(model.Id);

        return Json(isAllowed);
    }
}
