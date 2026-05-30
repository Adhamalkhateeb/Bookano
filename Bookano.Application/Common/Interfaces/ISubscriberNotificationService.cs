namespace Bookano.Application.Common.Interfaces;

public interface ISubscriberNotificationService
{
    Task SendWelcomeAsync(Subscriber subscriber);
    Task SendSubscriptionRenewalAsync(Subscription subscription);
}
