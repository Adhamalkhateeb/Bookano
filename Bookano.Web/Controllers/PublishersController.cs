using Bookano.Application.DTOs.Publishers;
using Bookano.Application.Services.Publishers;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class PublishersController(
        IMapper mapper,
        IPublisherService publisherService
    ) : Controller
    {
        private readonly IMapper _mapper = mapper;
        private readonly IPublisherService _publisherService = publisherService;

        public async Task<IActionResult> Index(CancellationToken ct = default)
        {
            var publishers = await _publisherService.GetAllAsync(ct);

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
        public async Task<IActionResult> Create(PublisherFormViewModel model, CancellationToken ct = default)
        {
            var dto = _mapper.Map<PublisherFormDto>(model);
            var result = await _publisherService.AddAsync(dto, ct);

            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            var vm = _mapper.Map<PublisherViewModel>(result.Value);

            return PartialView("_PublisherRow", vm);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
        {
            var publisherDto = await _publisherService.GetByIdAsync(id, ct);

            if (publisherDto is null)
                return NotFound();

            var vm = _mapper.Map<PublisherFormViewModel>(publisherDto);

            return PartialView("_Form", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PublisherFormViewModel model, CancellationToken ct = default)
        {
            var dto = _mapper.Map<PublisherFormDto>(model);
            var result = await _publisherService.UpdateAsync(model.Id, dto, ct);

            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            var vm = _mapper.Map<PublisherViewModel>(result.Value);

            return PartialView("_PublisherRow", vm);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct = default)
        {
            var lastUpdatedOnUtc = await _publisherService.ToggleAsync(id, ct);

            if (!lastUpdatedOnUtc.HasValue)
                return NotFound();

            return Ok(lastUpdatedOnUtc.Value.ToString());
        }

        public async Task<IActionResult> AllowItem(PublisherFormViewModel model, CancellationToken ct = default)
        {
            var dto = _mapper.Map<PublisherFormDto>(model);
            var isAllowed = await _publisherService.IsPublisherAllowedAsync(dto, ct);

            return Json(isAllowed);
        }
    }
}
