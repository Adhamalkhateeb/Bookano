using Bookano.Application.Interfaces;
using Bookano.Domain.Common.Constants;
using WhatsAppCloudApi;

namespace Bookano.Infrastructure.BackgroundServices
{
    public class SubscriptionJobs(
        IUnitOfWork unitOfWork,
        IEmailBodyBuilder emailBodyBuilder,
        IEmailSender emailSender,
        IWhatsAppService whatsAppService
    )
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEmailBodyBuilder _emailBodyBuilder = emailBodyBuilder;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IWhatsAppService _whatsAppService = whatsAppService;

        public async Task PrepareExpirationAlerts()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var soonThreshold = today.AddDays(5);

            var relevant = await _unitOfWork
                .Subscribers.GetQueryable()
                .Where(s => !s.IsBlackListed && s.Subscriptions.Any())
                .Select(s => new
                {
                    Subscriber = s,
                    LatestEnd = s.Subscriptions.Max(sb => sb.EndDate),
                })
                .Where(x => x.LatestEnd >= today && x.LatestEnd <= soonThreshold)
                .ToListAsync();

            var expiringSoon = relevant
                .Where(x => x.LatestEnd == soonThreshold)
                .Select(x => (x.Subscriber, x.LatestEnd))
                .ToList();

            var expiredToday = relevant
                .Where(x => x.LatestEnd == today)
                .Select(x => (x.Subscriber, x.LatestEnd))
                .ToList();

            await SendExpiringSoonAlerts(expiringSoon);
            await SendExpiredTodayAlerts(expiredToday);
        }

        private async Task SendExpiringSoonAlerts(
            List<(Subscriber Subscriber, DateOnly LatestEnd)> alerts
        )
        {
            foreach (var (subscriber, latestEnd) in alerts)
            {
                var endDateFormatted = latestEnd.ToString("d MMM, yyyy");

                var placeholders = new Dictionary<string, string>
                {
                    {
                        "imageUrl",
                        ImageUrls.ExpiringSoon
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
                    await _whatsAppService.SendWhatsApp(
                        subscriber.MobileNumber,
                        WhatsAppTemplates.SubscriptionExpiration,
                        [
                            new WhatsAppTextParameter { Text = subscriber.FirstName },
                            new WhatsAppTextParameter { Text = endDateFormatted },
                        ]
                    );
            }
        }

        private async Task SendExpiredTodayAlerts(
            List<(Subscriber Subscriber, DateOnly LatestEnd)> alerts
        )
        {
            foreach (var (subscriber, latestEnd) in alerts)
            {
                var endDateFormatted = latestEnd.ToString("d MMM, yyyy");

                var placeholders = new Dictionary<string, string>
                {
                    {
                        "imageUrl",
                        ImageUrls.Expired
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
                    await _whatsAppService.SendWhatsApp(
                        subscriber.MobileNumber,
                        WhatsAppTemplates.SubscriptionExpired,
                        [
                            new WhatsAppTextParameter { Text = subscriber.FirstName },
                            new WhatsAppTextParameter { Text = endDateFormatted },
                        ]
                    );
            }
        }
    }
}
