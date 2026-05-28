using Microsoft.Extensions.Options;

namespace Bookano.Infrastructure.Services
{
    public class EmailBodyBuilder(IOptions<MailSettings> mailSettings) : IEmailBodyBuilder
    {
        private readonly MailSettings _settings = mailSettings.Value;

        public string GetEmailBody(string template, Dictionary<string, string> placeholders)
        {
            var filePath = Path.Combine(_settings.TemplatesPath, $"{template}.html");

            using var sr = new StreamReader(filePath);
            var templateContent = sr.ReadToEnd();

            foreach (var placeholder in placeholders)
                templateContent = templateContent.Replace(
                    $"[{placeholder.Key}]",
                    placeholder.Value
                );

            return templateContent;
        }
    }
}
