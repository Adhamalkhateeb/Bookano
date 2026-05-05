using System.Composition;
using System.Formats.Asn1;
using Bookano.Web.Services.Image;
using CloudinaryDotNet.Actions;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Index), model);

            var subscriber = await _context.Subscribers.SingleOrDefaultAsync(s =>
                s.MobileNumber == model.Value
                || s.NationalId == model.Value
                || s.Email == model.Value
            );

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);

            return PartialView("_Result", viewModel);
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
            subscriber.ImagePublicId = uploadResult.PublicId!;
            subscriber.ImageThumbnailUrl = _imageService.GetThumbnail(uploadResult.PublicId!);
            subscriber.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Subscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var subscriber = await _context.Subscribers.FindAsync(id);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);

            return View("Form", await PopulateViewModelAsync(viewModel));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var subscriber = await _context.Subscribers.FindAsync(model.Id);

            if (subscriber is null)
                return NotFound();

            subscriber = _mapper.Map(model, subscriber);

            if (model.Image is not null)
            {
                var imageValidationError = _imageService.ValidateImage(model.Image);
                if (imageValidationError is not null)
                {
                    ModelState.AddModelError("Image", imageValidationError);
                    return View("Form", await PopulateViewModelAsync(model));
                }

                if (!string.IsNullOrEmpty(subscriber.ImagePublicId))
                {
                    var deleteResult = await _imageService.DeleteAsync(subscriber.ImagePublicId);
                    if (!deleteResult.IsSuccess)
                    {
                        ModelState.AddModelError("Image", deleteResult.ErrorMessage!);
                        return View("Form", await PopulateViewModelAsync(model));
                    }
                }

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
                subscriber.ImageThumbnailUrl = _imageService.GetThumbnail(uploadResult.PublicId!);
                subscriber.ImagePublicId = uploadResult.PublicId!;
            }

            subscriber.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
            subscriber.LastUpdatedOnUtc = DateTimeOffset.UtcNow;

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
