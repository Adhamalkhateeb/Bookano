using System.Text.Encodings.Web;

namespace Bookano.Web.Services.Mail
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string GetEmailBody(string template, Dictionary<string, string> placeholders)
        {
            var filePath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "templates",
                $"{template}.html"
            );
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
