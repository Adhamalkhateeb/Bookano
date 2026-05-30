using Microsoft.Extensions.Options;
using WhatsAppCloudApi;
using WhatsAppCloudApi.Services;

namespace Bookano.Infrastructure.Services
{
    public class WhatsAppService(
        IOptions<WhatsAppSettings> settings,
        IWhatsAppClient whatsAppClient
    ) : IWhatsAppService
    {
        private readonly WhatsAppSettings _settings = settings.Value;
        private readonly IWhatsAppClient _whatsAppClient = whatsAppClient;

        public async Task SendWhatsApp(
            string mobileNumber,
            string template,
            string languageCode,
            List<object>? parameters = null
        )
        {
            List<WhatsAppComponent>? components = null;

            if (parameters is not null && parameters.Count != 0)
            {
                components = [new WhatsAppComponent { Type = "body", Parameters = parameters }];
            }

            var usedMobileNumber = _settings.IsDevelopment
                ? _settings.DevelopmentOverrideMobile ?? mobileNumber
                : mobileNumber;

            await _whatsAppClient.SendMessage(
                $"2{usedMobileNumber}",
                languageCode,
                template,
                components
            );
        }
    }
}
