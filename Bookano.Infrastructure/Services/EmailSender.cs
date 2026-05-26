using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Bookano.Infrastructure.Services
{
    public class EmailSender(IOptions<MailSettings> mailSettings) : IEmailSender
    {
        private readonly MailSettings _mailSettings = mailSettings.Value;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using MailMessage message = new()
            {
                From = new MailAddress(_mailSettings.Email!, _mailSettings.DisplayName),
                Body = htmlMessage,
                Subject = subject,
                IsBodyHtml = true,
            };

            var recipient = _mailSettings.IsDevelopment
                ? _mailSettings.DevelopmentOverrideRecipient ?? email
                : email;

            message.To.Add(recipient);

            using SmtpClient smtpClient = new(_mailSettings.Host, _mailSettings.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_mailSettings.Email, _mailSettings.Password),
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}
