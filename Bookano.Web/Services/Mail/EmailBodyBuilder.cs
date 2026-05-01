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

        public string GetEmailBody(
            string imgaeUrl,
            string header,
            string body,
            string url,
            string linkTitle
        )
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "templates", "email.html");
            using var sr = new StreamReader(filePath);
            var template = sr.ReadToEnd();

            return template
                .Replace("[imageUrl]", imgaeUrl)
                .Replace("[header]", header)
                .Replace("[body]", body)
                .Replace("[url]", url)
                .Replace("[linkTitle]", linkTitle);
        }
    }
}
