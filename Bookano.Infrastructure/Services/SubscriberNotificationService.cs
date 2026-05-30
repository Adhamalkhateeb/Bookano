using Bookano.Application.Common.Models;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using WhatsAppCloudApi;

namespace Bookano.Infrastructure.Services;

public class SubscriberNotificationService(IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender,
    IWhatsAppService whatsAppService, IWebHostEnvironment webHostEnvironment) : ISubscriberNotificationService
{

    private readonly IEmailBodyBuilder _emailBodyBuilder = emailBodyBuilder;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IWhatsAppService _whatsAppService = whatsAppService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

    public async Task SendWelcomeAsync(Subscriber subscriber)
    {
        var placeholders = new Dictionary<string, string>
            {
                {
                    "imageUrl",
                    ImageUrls.Welcome
                },
                { "header", $"Welcome {subscriber.FirstName}" },
                { "body", "Thanks for joining Bookano 🤩" },
            };

        var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

        BackgroundJob.Enqueue(() =>
            _emailSender.SendEmailAsync(subscriber.Email, "Welcome to Bookano", body)
        );

        if (subscriber.HasWhatsApp)
        {
            List<object> parametres = [new WhatsAppTextParameter { Text = subscriber.FirstName }];
            var mobileNumber = _webHostEnvironment.IsDevelopment() ? "01021094971" : subscriber.MobileNumber;
             
            BackgroundJob.Enqueue(() =>
                _whatsAppService.SendWhatsApp(
                    $"2{mobileNumber}",
                    WhatsAppTemplates.WelcomeMessage,
                    WhatsAppLanguageCode.English_US,
                    parametres
                )
            );
        }
    }

    public async Task SendSubscriptionRenewalAsync(Subscription subscription)
    {
        var placeholders = new Dictionary<string, string>
            {
                {
                    "imageUrl",
                    ImageUrls.Welcome
                },
                { "header", $"Hello {subscription.Subscriber!.FirstName}" },
                {
                    "body",
                    $"your subscription has been renewed through {subscription.EndDate:d MMM, yyyy} 🎉🎉"
                },
            };

        var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

        BackgroundJob.Enqueue(() =>
            _emailSender.SendEmailAsync(subscription.Subscriber.Email, "Bookano Subscription Renewal", body)
        );

        if (subscription.Subscriber.HasWhatsApp)
        {

            List<object> parameters =
            [

                new WhatsAppTextParameter { Text = subscription.Subscriber.FirstName },
                new WhatsAppTextParameter
                {
                    Text = subscription.EndDate.ToString("d MMM, yyyy"),
                },
            ];
               
            var mobileNumber = _webHostEnvironment.IsDevelopment()
                ? "01021094971"
                : subscription.Subscriber.MobileNumber;

            BackgroundJob.Enqueue(() =>
                _whatsAppService.SendWhatsApp(
                    $"2{mobileNumber}",
                    WhatsAppTemplates.SubscriptionRenewal,
                    WhatsAppLanguageCode.English,
                    parameters
                )
            );
        }

    }
}
