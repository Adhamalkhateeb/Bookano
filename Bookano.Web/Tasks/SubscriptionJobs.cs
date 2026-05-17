using System.Threading.RateLimiting;
using Bookano.Web.Services.Mail;

namespace Bookano.Web.Tasks
{
    public class SubscriptionJobs
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        private readonly WhatsAppHelper _whatsAppHelper;

        public SubscriptionJobs(
            ApplicationDbContext context,
            IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender,
            WhatsAppHelper whatsAppHelper
        )
        {
            _context = context;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
            _whatsAppHelper = whatsAppHelper;
        }

        public async Task PrepareExpirationAlerts()
        {
            var today = DateTime.UtcNow.Date;
            var soonThreshold = today.AddDays(5);

            var relevant = await _context
                .Subscribers.AsNoTracking()
                .Where(s => !s.IsBlackListed && s.Subscriptions.Any())
                .Select(s => new
                {
                    Subscriber = s,
                    LatestEnd = s.Subscriptions.Max(sb => sb.EndDate),
                })
                .Where(x => x.LatestEnd >= today && x.LatestEnd <= soonThreshold)
                .ToListAsync();

            var expiringSoon = relevant
                .Where(x => x.LatestEnd.Date == soonThreshold)
                .Select(x => (x.Subscriber, x.LatestEnd))
                .ToList();

            var expiredToday = relevant
                .Where(x => x.LatestEnd.Date == today)
                .Select(x => (x.Subscriber, x.LatestEnd))
                .ToList();

            await SendExpiringSoonAlerts(expiringSoon);
            await SendExpiredTodayAlerts(expiredToday);
        }

        private async Task SendExpiringSoonAlerts(
            List<(Subscriber Subscriber, DateTime LatestEnd)> alerts
        )
        {
            foreach (var (subscriber, latestEnd) in alerts)
            {
                var endDateFormatted = latestEnd.ToString("d MMM, yyyy");

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
                    await _whatsAppHelper.SendWhatsApp(
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

        private async Task SendExpiredTodayAlerts(
            List<(Subscriber Subscriber, DateTime LatestEnd)> alerts
        )
        {
            foreach (var (subscriber, latestEnd) in alerts)
            {
                var endDateFormatted = latestEnd.ToString("d MMM, yyyy");

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
                    await _whatsAppHelper.SendWhatsApp(
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
    }
}
