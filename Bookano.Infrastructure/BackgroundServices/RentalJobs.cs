using System.Text;
using Bookano.Domain.Common.Consts;

namespace Bookano.Infrastructure.BackgroundServices
{
    public class RentalJobs(
        IApplicationDbContext context,
        IEmailBodyBuilder emailBodyBuilder,
        IEmailSender emailSender,
        IWhatsAppService<Subscriber> whatsAppService
    )
    {
        private readonly IApplicationDbContext _context = context;
        private readonly IEmailBodyBuilder _emailBodyBuilder = emailBodyBuilder;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IWhatsAppService<Subscriber> _whatsAppService = whatsAppService;

        public async Task SendExpiringSoonAlerts()
        {
            var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

            var rentals = await _context
                .Rentals.AsNoTracking()
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
                        "https://res.cloudinary.com/bookano/image/upload/v1778410949/schedule_2_aumjwi.png"
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
                        rental.Subscriber,
                        WhatsAppTemplates.RentalExpiringSoon
                    );
            }
        }
    }
}
