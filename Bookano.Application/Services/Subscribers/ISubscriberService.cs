using Bookano.Application.DTOs.Subscribers;

namespace Bookano.Application.Services.Subscribers;

public interface ISubscriberService
{
    Task<SubscriberSearchResultDto?> SearchAsync(string value, CancellationToken ct = default);
    Task<SubscriberDto?> GetDetails(int id, CancellationToken ct = default);
    Task<SubscriberFormDto?> GetFormAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(SubscriberFormDto dto, CancellationToken ct = default);
    Task<Result<int>> UpdateAsync(SubscriberFormDto dto, CancellationToken ct = default);
    Task<Result<SubscriptionDto>> RenewSubscriptionAsync(int id, CancellationToken ct = default);
    Task<bool> IsEmailAvailableAsync(int id, string email, CancellationToken ct = default);
    Task<bool> IsMobileNumberAvailableAsync(
        int id,
        string mobileNumber,
        CancellationToken ct = default
    );
    Task<bool> IsNationalIdAvailableAsync(
        int id,
        string nationalId,
        CancellationToken ct = default
    );
}
