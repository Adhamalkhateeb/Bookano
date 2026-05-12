using System.Threading.RateLimiting;
using Bookano.Web.Services.Mail;

namespace Bookano.Web.Tasks
{
    public class SubscriptionJobs
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public SubscriptionJobs(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            IWhatsAppClient whatsAppClient,
            IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender
        )
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _whatsAppClient = whatsAppClient;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }

        public async Task PrepareExpirationAlerts()
        {
            var today = DateTime.Today;
            var soonThreshold = today.AddDays(5);

            var relevant = await _context
                .Subscribers.Include(s => s.Subscriptions)
                .Where(s => !s.IsBlackListed && s.Subscriptions.Any())
                .Select(s => new
                {
                    Subscriber = s,
                    LatestEnd = s.Subscriptions.Max(sb => sb.EndDate),
                })
                .Where(x => x.LatestEnd >= today && x.LatestEnd <= soonThreshold)
                .ToListAsync();

            var expiredToday = relevant
                .Where(x => x.LatestEnd.Date == today)
                .Select(x => x.Subscriber)
                .ToList();

            var expiringSoon = relevant
                .Where(x => x.LatestEnd.Date == soonThreshold)
                .Select(x => x.Subscriber)
                .ToList();

            await SendExpiringSoonAlerts(expiringSoon);
            await SendExpiredTodayAlerts(expiredToday);
        }

        private async Task SendExpiringSoonAlerts(List<Subscriber> subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                var endDate = subscriber.Subscriptions.Max(sb => sb.EndDate);
                var endDateFormatted = endDate.ToString("d MMM, yyyy");

                var placeholders = new Dictionary<string, string>
                {
                    {
                        "imageUrl",
                        "https://res.cloudinary.com/bookano/image/upload/v1778410949/schedule_2_aumjwi.png"
                    },
                    { "header", $"Hello {subscriber.FirstName}" },
                    {
                        "body",
                        $"Your subscription will expire on {endDateFormatted} 😞 Renew it to keep enjoying Bookano!"
                    },
                };

                var body = _emailBodyBuilder.GetEmailBody(
                    EmailTemplates.Notification,
                    placeholders
                );

                await _emailSender.SendEmailAsync(
                    subscriber.Email,
                    "Subscription Expiring Soon",
                    body
                );

                if (subscriber.HasWhatsApp)
                    await SendWhatsApp(
                        subscriber,
                        WhatsAppTemplates.SubscriptionExpiration,
                        new List<object>
                        {
                            new WhatsAppTextParameter { Text = subscriber.FirstName },
                            new WhatsAppTextParameter { Text = endDateFormatted },
                        }
                    );
            }
        }

        private async Task SendExpiredTodayAlerts(List<Subscriber> subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                var endDate = subscriber.Subscriptions.Max(sb => sb.EndDate);
                var endDateFormatted = endDate.ToString("d MMM, yyyy");

                var placeholders = new Dictionary<string, string>
                {
                    {
                        "imageUrl",
                        "https://res.cloudinary.com/bookano/image/upload/v1778415113/expired_spifns.png"
                    },
                    { "header", $"Hello {subscriber.FirstName}" },
                    {
                        "body",
                        $"Your subscription has expired today ({endDateFormatted}) 😔 Renew now to continue borrowing books!"
                    },
                };

                var body = _emailBodyBuilder.GetEmailBody(
                    EmailTemplates.Notification,
                    placeholders
                );

                await _emailSender.SendEmailAsync(subscriber.Email, "Subscription Expired", body);

                if (subscriber.HasWhatsApp)
                    await SendWhatsApp(
                        subscriber,
                        WhatsAppTemplates.SubscriptionExpired,
                        new List<object>
                        {
                            new WhatsAppTextParameter { Text = subscriber.FirstName },
                            new WhatsAppTextParameter { Text = endDateFormatted },
                        }
                    );
            }
        }

        private async Task SendWhatsApp(
            Subscriber subscriber,
            string template,
            List<object> parameters
        )
        {
            var component = new List<WhatsAppComponent>
            {
                new WhatsAppComponent { Type = "body", Parameters = parameters },
            };

            var mobileNumber = _webHostEnvironment.IsDevelopment()
                ? "01021094971"
                : subscriber.MobileNumber;

            await _whatsAppClient.SendMessage(
                $"2{mobileNumber}",
                WhatsAppLanguageCode.English,
                template,
                component
            );
        }
    }
}
