using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Reception}")]
    public class AreasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AreasController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var areas = await _context
                .Areas.AsNoTracking()
                .Include(a => a.Governorate)
                .ToListAsync();

            var viewModel = _mapper.Map<IEnumerable<AreaViewModel>>(areas);

            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var viewModel = await PopulateGovernoratesAsync();
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AreaFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var area = _mapper.Map<Area>(model);
            area.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            await _context.Entry(area).Reference(a => a.Governorate).LoadAsync();
            var areaViewModel = _mapper.Map<AreaViewModel>(area);

            return PartialView("_AreaRow", areaViewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var area = await _context
                .Areas.AsNoTracking()
                .Include(a => a.Governorate)
                .SingleOrDefaultAsync(a => a.Id == id);

            if (area is null)
                return NotFound();

            var areaFormViewModel = _mapper.Map<AreaFormViewModel>(area);
            areaFormViewModel = await PopulateGovernoratesAsync(areaFormViewModel);

            return PartialView("_Form", areaFormViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AreaFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var area = await _context.Areas.FindAsync(model.Id);

            if (area is null)
                return NotFound();

            area = _mapper.Map(model, area);
            area.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            area.LastUpdatedOnUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _context.Entry(area).Reference(a => a.Governorate).LoadAsync();

            var areaViewModel = _mapper.Map<AreaViewModel>(area);

            return PartialView("_AreaRow", areaViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var area = await _context.Areas.FindAsync(id);

            if (area is null)
                return NotFound();

            area.IsDeleted = !area.IsDeleted;
            var updatedOn = DateTimeOffset.UtcNow;
            area.LastUpdatedOnUtc = updatedOn;
            area.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            await _context.SaveChangesAsync();

            return Ok(updatedOn.ToString("o"));
        }

        public async Task<IActionResult> AllowItem(AreaFormViewModel model)
        {
            var area = await _context.Areas.SingleOrDefaultAsync(a =>
                (a.Name == model.Name && a.GovernorateId == model.GovernorateId)
            );
            var isAllowed = area is null || area.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        private async Task<AreaFormViewModel> PopulateGovernoratesAsync(
            AreaFormViewModel? model = null
        )
        {
            var governorates = await _context.Governorates.AsNoTracking().ToListAsync();
            var viewModel = model ?? new AreaFormViewModel();

            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);
            return viewModel;
        }
    }
}
