namespace Bookano.Web.Services.Mail
{
    public interface IEmailBodyBuilder
    {
        string GetEmailBody(string template, Dictionary<string, string> placeholders);
    }
}
