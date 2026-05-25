using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Bookano.Web.Services.Mail
{
    public class EmailSender(
        IOptions<MailSettings> mailSettings,
        IWebHostEnvironment webHostEnvironment
    ) : IEmailSender
    {
        private readonly MailSettings _mailSettings = mailSettings.Value;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using MailMessage message = new()
            {
                From = new MailAddress(_mailSettings.Email!, _mailSettings.DisplayName),
                Body = htmlMessage,
                Subject = subject,
                IsBodyHtml = true,
            };

            var recipient = _webHostEnvironment.IsDevelopment()
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
