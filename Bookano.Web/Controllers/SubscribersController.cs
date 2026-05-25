using Bookano.Domain.Entities;
using Bookano.Web.Services.Image;
using Bookano.Web.Services.Mail;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController(
        IApplicationDbContext context,
        IDataProtectionProvider dataProtector,
        IWebHostEnvironment webHostEnvironment,
        IMapper mapper,
        IWhatsAppClient whatsAppClient,
        [FromKeyedServices("local")] IImageService imageService,
        IEmailBodyBuilder emailBodyBuilder,
        IEmailSender emailSender
    ) : Controller
    {
        private readonly IApplicationDbContext _context = context;
        private readonly IDataProtector _dataProtector = dataProtector.CreateProtector("security");
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IMapper _mapper = mapper;
        private readonly IWhatsAppClient _whatsAppClient = whatsAppClient;
        private readonly IImageService _imageService = imageService;
        private readonly IEmailBodyBuilder _emailBodyBuilder = emailBodyBuilder;
        private readonly IEmailSender _emailSender = emailSender;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = await _context.Subscribers.SingleOrDefaultAsync(s =>
                s.MobileNumber == model.Value
                || s.NationalId == model.Value
                || s.Email == model.Value
            );

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);
            if (subscriber is not null)
                viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);
        }

        public async Task<IActionResult> Details(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var subscriber = await _context
                .Subscribers.Include(s => s.Area)
                .Include(s => s.Governorate)
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                    .ThenInclude(r => r.RentalCopies)
                .SingleOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View("Form", await PopulateViewModelAsync());

        [HttpPost]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var subscriber = _mapper.Map<Subscriber>(model);

            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";
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

            var subscription = new Subscription
            {
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            };

            subscriber.Subscriptions.Add(subscription);

            _context.Subscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            var placeholders = new Dictionary<string, string>
            {
                {
                    "imageUrl",
                    "https://res.cloudinary.com/bookano/image/upload/v1777605605/icon-positive-vote-1_zw88ur.svg"
                },
                { "header", $"Welcome {subscriber.FirstName}" },
                { "body", "Thanks for joining Bookano 🤩" },
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() =>
                _emailSender.SendEmailAsync(subscriber.Email, "Welcome to Bookano", body)
            );

            if (subscriber.HasWhatsApp)
            {
                var component = new List<WhatsAppComponent>
                {
                    new()
                    {
                        Type = "body",
                        Parameters = [new WhatsAppTextParameter { Text = subscriber.FirstName }],
                    },
                };
                var mobileNumber = _webHostEnvironment.IsDevelopment()
                    ? "01021094971"
                    : subscriber.MobileNumber;

                BackgroundJob.Enqueue(() =>
                    _whatsAppClient.SendMessage(
                        $"2{mobileNumber}",
                        WhatsAppLanguageCode.English_US,
                        WhatsAppTemplates.WelcomeMessage,
                        component
                    )
                );
            }

            return RedirectToAction(
                nameof(Details),
                new { Id = _dataProtector.Protect(subscriber.Id.ToString()) }
            );
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = await _context.Subscribers.FindAsync(subscriberId);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
            viewModel.Key = id;

            return View("Form", await PopulateViewModelAsync(viewModel));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key!));
            var subscriber = await _context.Subscribers.FindAsync(subscriberId);

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

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { Id = model.Key });
        }

        [HttpPost]
        public async Task<IActionResult> RenewSubscription(string subscriberKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(subscriberKey));

            var subscriber = await _context
                .Subscribers.Include(s => s.Subscriptions)
                .SingleOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            if (subscriber.IsBlackListed)
                return BadRequest();

            var lastSubscription = subscriber.Subscriptions.OrderBy(s => s.EndDate).Last();

            var startDate =
                DateOnly.FromDateTime(DateTime.Today) > lastSubscription.EndDate
                    ? DateOnly.FromDateTime(DateTime.Today)
                    : lastSubscription.EndDate.AddDays(1);

            var newSubscription = new Subscription
            {
                StartDate = startDate,
                EndDate = startDate.AddYears(1),
            };

            subscriber.Subscriptions.Add(newSubscription);
            await _context.SaveChangesAsync();

            var placeholders = new Dictionary<string, string>
            {
                {
                    "imageUrl",
                    "https://res.cloudinary.com/bookano/image/upload/v1777605605/icon-positive-vote-1_zw88ur.svg"
                },
                { "header", $"Hello {subscriber.FirstName}" },
                {
                    "body",
                    $"your subscription has been renewed through {newSubscription.EndDate:d MMM, yyyy} 🎉🎉"
                },
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() =>
                _emailSender.SendEmailAsync(subscriber.Email, "Bookano Subscription Renewal", body)
            );

            if (subscriber.HasWhatsApp)
            {
                var component = new List<WhatsAppComponent>
                {
                    new()
                    {
                        Type = "body",
                        Parameters =
                        [
                            new WhatsAppTextParameter { Text = subscriber.FirstName },
                            new WhatsAppTextParameter
                            {
                                Text = newSubscription.EndDate.ToString("d MMM, yyyy"),
                            },
                        ],
                    },
                };
                var mobileNumber = _webHostEnvironment.IsDevelopment()
                    ? "01021094971"
                    : subscriber.MobileNumber;

                BackgroundJob.Enqueue(() =>
                    _whatsAppClient.SendMessage(
                        $"2{mobileNumber}",
                        WhatsAppLanguageCode.English,
                        WhatsAppTemplates.SubscriptionRenewal,
                        component
                    )
                );
            }

            var viewModel = _mapper.Map<SubscriptionViewModel>(newSubscription);
            return PartialView("_SubscriptionRow", viewModel);
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
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = await _context.Subscribers.SingleOrDefaultAsync(s =>
                s.Email == model.Email
            );
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowMobileNumber(SubscriberFormViewModel model)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = await _context.Subscribers.SingleOrDefaultAsync(s =>
                s.MobileNumber == model.MobileNumber
            );
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowNationalId(SubscriberFormViewModel model)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = await _context.Subscribers.SingleOrDefaultAsync(s =>
                s.NationalId == model.NationalId
            );
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);

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
