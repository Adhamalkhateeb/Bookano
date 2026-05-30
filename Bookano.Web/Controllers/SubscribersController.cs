using Bookano.Application.DTOs.Subscribers;
using Bookano.Application.Services.Areas;
using Bookano.Application.Services.Subscribers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController(
        IDataProtectionProvider dataProtector,
        IMapper mapper,
        ISubscriberService subscriberService,
        IGovernorateService governorateService,
        IAreaService areaService
    ) : Controller
    {
        private readonly IDataProtector _dataProtector = dataProtector.CreateProtector("security");
        private readonly IMapper _mapper = mapper;
        private readonly ISubscriberService _subscriberService = subscriberService;
        private readonly IGovernorateService _governorateService = governorateService;
        private readonly IAreaService _areaService = areaService;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(SearchFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = await _subscriberService.SearchAsync(model.Value, ct);

            if (subscriber is null)
                return PartialView("_Result", null);

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);
            viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);
        }

        public async Task<IActionResult> Details(string id,CancellationToken ct)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = await _subscriberService.GetDetails(subscriberId, ct);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View("Form", await PopulateViewModelAsync());

        [HttpPost]
        public async Task<IActionResult> Create(SubscriberFormViewModel model, CancellationToken ct)
        {
            var dto = _mapper.Map<SubscriberFormDto>(model);

            if (model.Image is not null)
            {
                dto.Image = new ImageUploadDto
                {
                    Stream = model.Image.OpenReadStream(),
                    FileName = model.Image.FileName,
                    Length = model.Image.Length,
                };
            }

            var result = await _subscriberService.CreateAsync(dto, ct);
            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            return RedirectToAction(
                nameof(Details),
                new { Id = _dataProtector.Protect(result.Value!.ToString()) }
            );
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id,CancellationToken ct)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var subscriber = await _subscriberService.GetFormAsync(subscriberId, ct);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
            viewModel.Key = id;

            return View("Form", await PopulateViewModelAsync(viewModel));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model, CancellationToken ct)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key!));

            var dto = _mapper.Map<SubscriberFormDto>(model);
            dto.Id = subscriberId;

            if (model.Image is not null)
            {
                dto.Image = new ImageUploadDto
                {
                    Stream = model.Image.OpenReadStream(),
                    FileName = model.Image.FileName,
                    Length = model.Image.Length,
                };
            }

            var result = await _subscriberService.UpdateAsync(dto, ct);
            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            return RedirectToAction(nameof(Details), new { Id = model.Key });
        }

        [HttpPost]
        public async Task<IActionResult> RenewSubscription(string subscriberKey, CancellationToken ct)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(subscriberKey));
            var result = await _subscriberService.RenewSubscriptionAsync(subscriberId, ct);

            if (!result.IsSuccess)
                return result.ErrorMessage == Error.BlackListedSubscriber ? BadRequest() : NotFound();

            var viewModel = _mapper.Map<SubscriptionViewModel>(result.Value);
            return PartialView("_SubscriptionRow", viewModel);
        }

        [AjaxOnly]
        public async Task<IActionResult> GetAreas(int governorateId)
        {
            var areas = await _areaService.GetGovernorateAreasAsync(governorateId);

            return Ok(_mapper.Map<IEnumerable<SelectListItem>>(areas));
        }

        public async Task<IActionResult> AllowEmail(SubscriberFormViewModel model, CancellationToken ct)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var isAllowed = await _subscriberService.IsEmailAvailableAsync(subscriberId, model.Email, ct);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowMobileNumber(SubscriberFormViewModel model, CancellationToken ct)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var isAllowed = await _subscriberService.IsMobileNumberAvailableAsync(
                subscriberId,
                model.MobileNumber,
                ct
            );

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowNationalId(SubscriberFormViewModel model, CancellationToken ct)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var isAllowed = await _subscriberService.IsNationalIdAvailableAsync(
                subscriberId,
                model.NationalId,
                ct
            );

            return Json(isAllowed);
        }

        private async Task<SubscriberFormViewModel> PopulateViewModelAsync(
            SubscriberFormViewModel? model = null,CancellationToken ct = default
        )
        {
            model ??= new SubscriberFormViewModel();

            var governorates = await _governorateService.GetAllAsync(ct);

            model.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            if (model.GovernorateId > 0)
            {
                var areas = await _areaService.GetAllAsync();
                model.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return model;
        }

    }
}
