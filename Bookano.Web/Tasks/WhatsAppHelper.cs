namespace Bookano.Web.Tasks
{
    public class WhatsAppHelper
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWhatsAppClient _whatsAppClient;

        public WhatsAppHelper(
            IWebHostEnvironment webHostEnvironment,
            IWhatsAppClient whatsAppClient
        )
        {
            _webHostEnvironment = webHostEnvironment;
            _whatsAppClient = whatsAppClient;
        }

        public async Task SendWhatsApp(
            Subscriber subscriber,
            string template,
            List<object>? parameters = null
        )
        {
            List<WhatsAppComponent>? components = null;

            if (parameters is not null && parameters.Any())
            {
                components = [new WhatsAppComponent { Type = "body", Parameters = parameters }];
            }

            var mobileNumber = _webHostEnvironment.IsDevelopment()
                ? "01021094971"
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
