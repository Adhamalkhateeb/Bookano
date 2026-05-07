namespace Bookano.Web.Services.Mail
{
    public interface IEmailBodyBuilder
    {
        string GetEmailBody(
            string imgaeUrl,
            string header,
            string body,
            string url,
            string linkTitle
        );
    }
}
