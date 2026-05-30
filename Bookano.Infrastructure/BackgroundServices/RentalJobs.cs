using System.Text;
using Bookano.Application.Interfaces;
using WhatsAppCloudApi;

namespace Bookano.Infrastructure.BackgroundServices
{
    public class RentalJobs(
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

        public async Task SendExpiringSoonAlerts()
        {
            var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

            var rentals = await _unitOfWork
                .Rentals.GetQueryable()
                .Include(r => r.Subscriber)
                .Include(r => r.RentalCopies)
                    .ThenInclude(rc => rc.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .Where(r =>
                    r.RentalCopies.Any(rc => !rc.ReturnDate.HasValue && rc.EndDate == tomorrow)
                )
                .ToListAsync();

            foreach (var rental in rentals)
            {
                var expiringCopies = rental
                    .RentalCopies.Where(rc => !rc.ReturnDate.HasValue && rc.EndDate == tomorrow)
                    .ToList();

                var message = new StringBuilder();
                message.AppendLine(
                    $"Your rental for the below book(s) will expire tomorrow {tomorrow:dd MMM, yyyy}:"
                );
                message.AppendLine("<ul>");

                foreach (var copy in expiringCopies)
                    message.AppendLine(
                        $"<li style='text-align: left'>{copy.BookCopy!.Book!.Title}</li>"
                    );

                message.AppendLine("</ul>");

                var placeholders = new Dictionary<string, string>
                {
                    {
                        "imageUrl",
                        ImageUrls.ExpiringSoon
                    },
                    { "header", $"Hello {rental.Subscriber!.FirstName}, " },
                    { "body", message.ToString() },
                };

                var body = _emailBodyBuilder.GetEmailBody(
                    EmailTemplates.Notification,
                    placeholders
                );

                await _emailSender.SendEmailAsync(
                    rental.Subscriber.Email,
                    "Reminder: Book Return Due Tomorrow",
                    body
                );

                if (rental.Subscriber.HasWhatsApp)
                    await _whatsAppService.SendWhatsApp(
                        rental.Subscriber.MobileNumber,
                        WhatsAppTemplates.RentalExpiringSoon,
                        WhatsAppLanguageCode.English
                    );
            }
        }
    }
}
