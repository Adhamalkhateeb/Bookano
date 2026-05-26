namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class AuthorsController(
        IApplicationDbContext context,
        IMapper mapper,
        IValidator<AuthorFormViewModel> validator
    ) : Controller
    {
        private readonly IApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<AuthorFormViewModel> _validator = validator;

        public async Task<IActionResult> Index()
        {
            var authors = await _context.Authors.AsNoTracking().ToListAsync();

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuthorFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
            validationResult.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            var author = _mapper.Map<Author>(model);

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var authorViewModel = _mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", authorViewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author is null)
                return NotFound();

            var authorFormViewModel = _mapper.Map<AuthorFormViewModel>(author);

            return PartialView("_Form", authorFormViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AuthorFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
            validationResult.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            var author = await _context.Authors.FindAsync(model.Id);

            if (author is null)
                return NotFound();

            author = _mapper.Map(model, author);
            await _context.SaveChangesAsync();

            var authorViewModel = _mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", authorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author is null)
                return NotFound();

            author.IsDeleted = !author.IsDeleted;
            await _context.SaveChangesAsync();

            return Ok(author.LastUpdatedOnUtc.ToString());
        }

        public async Task<IActionResult> AllowItem(AuthorFormViewModel model)
        {
            var author = await _context.Authors.SingleOrDefaultAsync(c => c.Name == model.Name);
            var isAllowed = author is null || author.Id.Equals(model.Id);

            return Json(isAllowed);
        }
    }
}
