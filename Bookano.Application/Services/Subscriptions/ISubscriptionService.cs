

using Bookano.Application.DTOs.Subscribers;

namespace Bookano.Application.Services.Subscriptions;

public interface ISubscriptionService
{
    Task<IEnumerable<SubscriptionDto>> GetBySubscriberAsync(int subscriberId, CancellationToken ct = default);
    Task<Result<SubscriptionDto>> RenewAsync(int subscriberId, CancellationToken ct = default);
    Task<IEnumerable<SubscriberDto>> GetSubscribersWithSubscriptionAsync(CancellationToken ct = default);
    Task<IEnumerable<SubscriberDto>> GetExpiredSubscribersAsync(CancellationToken ct = default);
    Task<IEnumerable<SubscriberDto>> GetSubscribersNearingExpiryAsync(int days = 7, CancellationToken ct = default);

}
