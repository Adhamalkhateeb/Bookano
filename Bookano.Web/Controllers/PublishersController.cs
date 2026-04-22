namespace Bookano.Web.Controllers
{
    public class PublishersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PublishersController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var publishers = await _context.Publishers.AsNoTracking().ToListAsync();

            var viewModel = _mapper.Map<IEnumerable<PublisherViewModel>>(publishers);

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
        public async Task<IActionResult> Create(PublisherFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var publisher = _mapper.Map<Publisher>(model);

            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();

            var PublisherViewModel = _mapper.Map<PublisherViewModel>(publisher);

            return PartialView("_PublisherRow", PublisherViewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var Publisher = await _context.Publishers.FindAsync(id);

            if (Publisher is null)
                return NotFound();

            var PublisherFormViewModel = _mapper.Map<PublisherFormViewModel>(Publisher);

            return PartialView("_Form", PublisherFormViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PublisherFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var Publisher = await _context.Publishers.FindAsync(model.Id);

            if (Publisher is null)
                return NotFound();

            Publisher = _mapper.Map(model, Publisher);
            Publisher.LastUpdatedOnUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var PublisherViewModel = _mapper.Map<PublisherViewModel>(Publisher);

            return PartialView("_PublisherRow", PublisherViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var Publisher = await _context.Publishers.FindAsync(id);

            if (Publisher is null)
                return NotFound();

            Publisher.IsDeleted = !Publisher.IsDeleted;
            Publisher.LastUpdatedOnUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(Publisher.LastUpdatedOnUtc.ToLocalFormat());
        }

        public async Task<IActionResult> AllowItem(PublisherFormViewModel model)
        {
            var Publisher = await _context.Publishers.SingleOrDefaultAsync(c =>
                c.Name == model.Name
            );
            var isAllowed = Publisher is null || Publisher.Id.Equals(model.Id);

            return Json(isAllowed);
        }
    }
}
