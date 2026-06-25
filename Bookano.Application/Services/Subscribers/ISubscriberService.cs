using Bookano.Application.DTOs.Dashboard;
using Bookano.Application.DTOs.Subscribers;

namespace Bookano.Application.Services.Subscribers;

public interface ISubscriberService
{
    Task<int> GetActiveSubscribersCountAsync(CancellationToken ct = default);
    Task<SubscriberDto?> SearchAsync(string value, CancellationToken ct = default);
    Task<SubscriberDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(SubscriberSaveDto dto, CancellationToken ct = default);
    Task<Result<int>> UpdateAsync(SubscriberSaveDto dto, CancellationToken ct = default);

    Task<bool> IsEmailAvailableAsync(string email, int excludeId, CancellationToken ct = default);
    Task<bool> IsMobileNumberAvailableAsync(string mobileNumber, int excludeId, CancellationToken ct = default);
    Task<bool> IsNationalIdAvailableAsync(string nationalId, int excludeId, CancellationToken ct = default);

    Task<IEnumerable<SubscriberDto>> GetSubscribersWithRentalsAsync(CancellationToken ct = default);
    Task<IEnumerable<SubscriberDto>> GetSubscribersWithOverdueRentalsAsync(CancellationToken ct = default);
    Task<bool> CanRentAsync(int subscriberId, CancellationToken ct = default);

    Task<IEnumerable<ChartItemDto>> GetSubscribersPerGovernorateAsync(CancellationToken ct = default);
}
