using System.Formats.Asn1;
using Bookano.Web.Services.Image;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public SubscribersController(
            ApplicationDbContext context,
            IMapper mapper,
            [FromKeyedServices("local")] IImageService imageService
        )
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View("Form", await PopulateViewModelAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var subscriber = _mapper.Map<Subscriber>(model);

            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
            var uploadResult = await _imageService.UploadAsync(
                model.Image,
                "subscribers",
                imageName
            );

            if (!uploadResult.IsSuccess)
            {
                ModelState.AddModelError("Image", uploadResult.ErrorMessage!);
                return View("Form", await PopulateViewModelAsync(model));
            }

            subscriber.ImageUrl = uploadResult.Url!;
            subscriber.ImageThumnailUrl = _imageService.GetThumbnail(uploadResult.PublicId!);
            subscriber.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Subscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [AjaxOnly]
        public async Task<IActionResult> GetAreas(int governorateId)
        {
            var areas = await _context
                .Areas.AsNoTracking()
                .Where(a => a.Governorate!.Id == governorateId && !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<SelectListItem>>(areas));
        }

        public async Task<IActionResult> AllowEmail(SubscriberFormViewModel model)
        {
            var subscriber = await _context.Subscribers.SingleOrDefaultAsync(s =>
                s.Email == model.Email
            );
            var isAllowed = subscriber is null || subscriber.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowMobileNumber(SubscriberFormViewModel model)
        {
            var subscriber = await _context.Subscribers.SingleOrDefaultAsync(s =>
                s.MobileNumber == model.MobileNumber
            );
            var isAllowed = subscriber is null || subscriber.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowNationalId(SubscriberFormViewModel model)
        {
            var subscriber = await _context.Subscribers.SingleOrDefaultAsync(s =>
                s.NationalId == model.NationalId
            );
            var isAllowed = subscriber is null || subscriber.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        private async Task<SubscriberFormViewModel> PopulateViewModelAsync(
            SubscriberFormViewModel? model = null
        )
        {
            model ??= new SubscriberFormViewModel();

            var governorates = await _context
                .Governorates.AsNoTracking()
                .Where(g => !g.IsDeleted)
                .OrderBy(g => g.Name)
                .ToListAsync();

            model.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            if (model.GovernorateId > 0)
            {
                var areas = await _context
                    .Areas.AsNoTracking()
                    .Where(a => a.GovernorateId == model.GovernorateId && !a.IsDeleted)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                model.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return model;
        }
    }
}
