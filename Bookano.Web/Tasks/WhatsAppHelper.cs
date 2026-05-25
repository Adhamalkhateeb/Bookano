namespace Bookano.Web.Tasks
{
    public class WhatsAppHelper(
        IWebHostEnvironment webHostEnvironment,
        IWhatsAppClient whatsAppClient
    )
    {
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
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
