using Microsoft.Extensions.Options;
using WhatsAppCloudApi;
using WhatsAppCloudApi.Services;

namespace Bookano.Infrastructure.Services
{
    public class WhatsAppService(
        IOptions<WhatsAppSettings> settings,
        IWhatsAppClient whatsAppClient
    ) : IWhatsAppService<Subscriber>
    {
        private readonly WhatsAppSettings _settings = settings.Value;
        private readonly IWhatsAppClient _whatsAppClient = whatsAppClient;

        public async Task SendWhatsApp(
            Subscriber subscriber,
            string template,
            List<object>? parameters = null
        )
        {
            List<WhatsAppComponent>? components = null;

            if (parameters is not null && parameters.Count != 0)
            {
                components = [new WhatsAppComponent { Type = "body", Parameters = parameters }];
            }

            var mobileNumber = _settings.IsDevelopment
                ? _settings.DevelopmentOverrideMobile ?? subscriber.MobileNumber
                : subscriber.MobileNumber;

            await _whatsAppClient.SendMessage(
                $"2{mobileNumber}",
                WhatsAppLanguageCode.English,
                template,
                components
            );
        }
    }
}
