using Hangfire;

namespace Bookano.Infrastructure.Services;

public class UserNotificationService(IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender) : IUserNotificationService
{
    private readonly IEmailBodyBuilder _emailBodyBuilder = emailBodyBuilder;
    private readonly IEmailSender _emailSender = emailSender;

    public async Task SendWelcomeEmailAsync(string email, string fullName, string callbackUrl)
    {
        var placeholders = new Dictionary<string, string>
        {
            { "imageUrl", ImageUrls.Welcome },
            { "header", $"Hey {fullName}, thanks for joining us!" },
            { "body", "Please confirm your email" },
            { "url", callbackUrl },
            { "linkTitle", "Active Account!" },
        };

        var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Email, placeholders);

        BackgroundJob.Enqueue(() =>
            _emailSender.SendEmailAsync(email, "Confirm your email", body)
        );

        await Task.CompletedTask;
    }

    public async Task SendPasswordResetEmailAsync(string email, string fullName, string callbackUrl)
    {
        var placeholders = new Dictionary<string, string>
        {
            { "imageUrl", ImageUrls.AccountSettings },
            { "header", $"Hey {fullName}" },
            { "body", "We received a request to reset your password. You can do so by clicking the button below:" },
            { "url", callbackUrl },
            { "linkTitle", "Reset Password!" },
        };

        var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Email, placeholders);

        BackgroundJob.Enqueue(() =>
            _emailSender.SendEmailAsync(email, "Reset Password", body)
        );

        await Task.CompletedTask;
    }

    public async Task SendEmailConfirmationAsync(string email, string fullName, string callbackUrl)
    {
        var placeholders = new Dictionary<string, string>
        {
            { "imageUrl", ImageUrls.Welcome },
            { "header", $"Hey {fullName}, thanks for joining us!" },
            { "body", "Please confirm your email" },
            { "url", callbackUrl },
            { "linkTitle", "Active Account!" },
        };

        var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Email, placeholders);

        BackgroundJob.Enqueue(() =>
            _emailSender.SendEmailAsync(email, "Confirm your email", body)
        );

        await Task.CompletedTask;
    }

    public async Task SendEmailChangeConfirmationAsync(string newEmail, string fullName, string callbackUrl)
    {
        var placeholders = new Dictionary<string, string>
        {
            { "imageUrl", ImageUrls.AccountSettings },
            { "header", $"Hey {fullName}" },
            { "body", "We received a request to change your email. You can do so by clicking the button below:" },
            { "url", callbackUrl },
            { "linkTitle", "Change Email" },
        };

        var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Email, placeholders);

        BackgroundJob.Enqueue(() =>
            _emailSender.SendEmailAsync(newEmail, "Change your email", body)
        );

        await Task.CompletedTask;
    }
}
