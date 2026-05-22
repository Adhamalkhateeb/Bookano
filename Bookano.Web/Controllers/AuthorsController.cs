namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuthorsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

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
        public async Task<IActionResult> Create(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = _mapper.Map<Author>(model);
            author.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

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
        public async Task<IActionResult> Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = await _context.Authors.FindAsync(model.Id);

            if (author is null)
                return NotFound();

            author = _mapper.Map(model, author);
            author.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            author.LastUpdatedOnUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var authorViewModel = _mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", authorViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author is null)
                return NotFound();

            author.IsDeleted = !author.IsDeleted;
            var updatedOn = DateTimeOffset.UtcNow;
            author.LastUpdatedOnUtc = updatedOn;
            author.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            await _context.SaveChangesAsync();

            return Ok(updatedOn.ToString("o"));
        }

        public async Task<IActionResult> AllowItem(AuthorFormViewModel model)
        {
            var author = await _context.Authors.SingleOrDefaultAsync(c => c.Name == model.Name);
            var isAllowed = author is null || author.Id.Equals(model.Id);

            return Json(isAllowed);
        }
    }
}
