using Bookano.Application.DTOs.Areas;
using Bookano.Application.Services.Areas;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class AreasController(
        IMapper mapper,
         IAreaService areaService,
         IGovernorateService governorateService
    ) : Controller
    {
        private readonly IMapper _mapper = mapper;
        private readonly IAreaService _areaService = areaService;
        private readonly IGovernorateService _governorateService = governorateService;

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var areas = await _areaService.GetAllAsync(ct);

            var viewModel = _mapper.Map<IEnumerable<AreaViewModel>>(areas);

            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var viewModel = await PopulateGovernoratesAsync();
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AreaFormViewModel model,CancellationToken ct)
        {
            var dto = _mapper.Map<AreaFormDto>(model);

            var result = await _areaService.AddAsync(dto,ct); 

            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();
            
            var vm = _mapper.Map<AreaViewModel>(result.Value);

            return PartialView("_AreaRow", vm);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id,CancellationToken ct)
        {
            var area = await _areaService.GetAsync(id, ct);

            if (area is null)
                return NotFound();

            var vm = _mapper.Map<AreaFormViewModel>(area);
            vm = await PopulateGovernoratesAsync(vm);

            return PartialView("_Form", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AreaFormViewModel model,CancellationToken ct)
        {
            var dto = _mapper.Map<AreaFormDto>(model);
            
            var result = await _areaService.UpdateAsync(model.Id,dto,ct);

            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            var vm = _mapper.Map<AreaViewModel>(result.Value);
            return PartialView("_AreaRow", vm);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
        {
            var lastUpdatedOnUtc = await _areaService.ToggleAsync(id,ct);

            if (!lastUpdatedOnUtc.HasValue)
                return NotFound();

            return Ok(lastUpdatedOnUtc.Value.ToString());
        }

        public async Task<IActionResult> AllowItem(AreaFormViewModel model,CancellationToken ct)
        {
            var isAllowed = await _areaService.IsAreaAvailableAsync(model.Id,model.GovernorateId, model.Name,ct);

            return Json(isAllowed);
        }

        private async Task<AreaFormViewModel> PopulateGovernoratesAsync(
            AreaFormViewModel? model = null
        )
        {
            var governorates = await _governorateService.GetAllAsync();

            var viewModel = model ?? new AreaFormViewModel();
            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            return viewModel;
        }
    }
}
