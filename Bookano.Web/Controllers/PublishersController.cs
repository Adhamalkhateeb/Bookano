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

        public IActionResult Index()
        {
            var publishers = _context.Publishers.AsNoTracking().ToList();

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
        public IActionResult Create(PublisherFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var publisher = _mapper.Map<Publisher>(model);

            _context.Publishers.Add(publisher);
            _context.SaveChanges();

            var PublisherViewModel = _mapper.Map<PublisherViewModel>(publisher);

            return PartialView("_PublisherRow", PublisherViewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var Publisher = _context.Publishers.Find(id);

            if (Publisher is null)
                return NotFound();

            var PublisherFormViewModel = _mapper.Map<PublisherFormViewModel>(Publisher);

            return PartialView("_Form", PublisherFormViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PublisherFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var Publisher = _context.Publishers.Find(model.Id);

            if (Publisher is null)
                return NotFound();

            Publisher = _mapper.Map(model, Publisher);
            Publisher.LastUpdatedOnUtc = DateTime.UtcNow;
            _context.SaveChanges();

            var PublisherViewModel = _mapper.Map<PublisherViewModel>(Publisher);

            return PartialView("_PublisherRow", PublisherViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var Publisher = _context.Publishers.Find(id);

            if (Publisher is null)
                return NotFound();

            Publisher.IsDeleted = !Publisher.IsDeleted;
            Publisher.LastUpdatedOnUtc = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok(
                Publisher
                    .LastUpdatedOnUtc.GetValueOrDefault()
                    .ToLocalTime()
                    .ToString("yyyy/MM/dd hh:mm tt")
            );
        }

        public IActionResult AllowItem(PublisherFormViewModel model)
        {
            var Publisher = _context.Publishers.SingleOrDefault(c => c.Name == model.Name);
            var isAllowed = Publisher is null || Publisher.Id.Equals(model.Id);

            return Json(isAllowed);
        }
    }
}
