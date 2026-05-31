namespace Bookano.Application.Common.Interfaces;

public interface IUserNotificationService
{
    Task SendWelcomeEmailAsync(string email, string fullName, string callbackUrl);

    Task SendPasswordResetEmailAsync(string email, string fullName, string callbackUrl);

    Task SendEmailConfirmationAsync(string email, string fullName, string callbackUrl);

    Task SendEmailChangeConfirmationAsync(string newEmail, string fullName, string callbackUrl);
}
