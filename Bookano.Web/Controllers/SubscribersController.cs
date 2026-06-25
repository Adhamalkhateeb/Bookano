using Bookano.Application.DTOs.Subscribers;
using Bookano.Application.Services.Areas;
using Bookano.Application.Services.Subscribers;
using Bookano.Web.ViewModels.Subscribers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bookano.Web.ViewModels.Rentals;
using Bookano.Application.Services.Rentals;
using Bookano.Application.Services.Subscriptions;
using Bookano.Application.Services.Governorates;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController(
        IMapper mapper,
        ISubscriberService subscriberService,
        IGovernorateService governorateService,
        IAreaService areaService,
        IRentalService rentalService,
        ISubscriptionService subscriptionService,
        IWebHostEnvironment env,
        IValidator<SubscriberFormViewModel> validator,
        IDataProtectionProvider protector
    ) : Controller
    {
        private readonly IMapper _mapper = mapper;
        private readonly ISubscriberService _subscriberService = subscriberService;
        private readonly IGovernorateService _governorateService = governorateService;
        private readonly IAreaService _areaService = areaService;
        private readonly IRentalService _rentalService = rentalService;
        private readonly ISubscriptionService _subscriptionService = subscriptionService;
        private readonly IWebHostEnvironment _env = env;
        private readonly IValidator<SubscriberFormViewModel> _validator = validator;
        private readonly IDataProtector _protector = protector.CreateProtector("sec");

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
            viewModel.Key = _protector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);
        }

        public async Task<IActionResult> Details(string id, CancellationToken ct)
        {
            var subscriberId = int.Parse(_protector.Unprotect(id));
            var subscriber = await _subscriberService.GetByIdAsync(subscriberId, ct);

            if (subscriber is null)
                return NotFound();

            var subscriptions = await _subscriptionService.GetBySubscriberAsync(subscriberId, ct);
            var rentals = await _rentalService.GetBySubscriberAsync(subscriberId, ct);
            var canRent = await _subscriberService.CanRentAsync(subscriberId, ct);

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id;
            viewModel.Subscriptions = _mapper.Map<IEnumerable<SubscriptionViewModel>>(subscriptions);
            viewModel.Rentals = _mapper.Map<IEnumerable<RentalViewModel>>(rentals);
            viewModel.CanAddRental = canRent;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            viewModel.Status = subscriber.IsBlackListed ? SubscriberStatus.Banned
                : (!viewModel.Subscriptions.Any() || DateOnly.FromDateTime(viewModel.LastSubscriptionEndDate!.Value) < today)
                ? SubscriberStatus.Inactive : SubscriberStatus.Active;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View("Form", await PopulateViewModelAsync());

        [HttpPost]
        public async Task<IActionResult> Create(SubscriberFormViewModel model, CancellationToken ct)
        {
            var dto = _mapper.Map<SubscriberSaveDto>(model);

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
                new { Id = _protector.Protect(result.Value!.ToString()) }
            );
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id, CancellationToken ct)
        {
            var subscriberId = int.Parse(_protector.Unprotect(id));

            var subscriber = await _subscriberService.GetByIdAsync(subscriberId, ct);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
            viewModel.Key = id;

            return View("Form", await PopulateViewModelAsync(viewModel));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model, CancellationToken ct)
        {
            var subscriberId = int.Parse(_protector.Unprotect(model.Key!));

            var dto = _mapper.Map<SubscriberSaveDto>(model);
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
            var subscriberId = int.Parse(_protector.Unprotect(subscriberKey));
            var result = await _subscriptionService.RenewAsync(subscriberId, ct);

            if (result.IsFailure)
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
            var subscriberId = GetSubscriberId(model.Key);

            var isAllowed = await _subscriberService.IsEmailAvailableAsync(model.Email, subscriberId, ct);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowMobileNumber(SubscriberFormViewModel model, CancellationToken ct)
        {
            var subscriberId = GetSubscriberId(model.Key);

            var isAllowed = await _subscriberService.IsMobileNumberAvailableAsync(
                model.MobileNumber,
                subscriberId,
                ct
            );

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowNationalId(SubscriberFormViewModel model, CancellationToken ct)
        {
            var subscriberId = GetSubscriberId(model.Key);

            var isAllowed = await _subscriberService.IsNationalIdAvailableAsync(
                model.NationalId,
                subscriberId,
                ct
            );

            return Json(isAllowed);
        }

        private async Task<SubscriberFormViewModel> PopulateViewModelAsync(SubscriberFormViewModel? model = null,CancellationToken ct = default)
        {
            model ??= new SubscriberFormViewModel();

            var governorates = await _governorateService.GetActiveAsync(ct);

            model.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            if (model.GovernorateId > 0)
            {
                var areas = await _areaService.GetAllAsync();
                model.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return model;
        }

        private int GetSubscriberId(string? key) => key == null ? 0 : int.Parse(_protector.Unprotect(key));
    }
}
