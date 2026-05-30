namespace Bookano.Application.Common.Interfaces;

public interface IWhatsAppService
{
    Task SendWhatsApp(string mobileNumber, string template, string languageCode, List<object>? parameters = null);
}
