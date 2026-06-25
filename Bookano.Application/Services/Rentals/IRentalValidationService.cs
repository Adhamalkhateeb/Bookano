using Bookano.Domain.Entities;
using Bookano.Application.DTOs.Rentals;

namespace Bookano.Application.Services.Rentals;

public interface IRentalValidationService
{
    Task<Result<int>> CheckSubscriberEligibilityAsync(int subscriberId, int? excludeRentalId = null, CancellationToken ct = default);
    Task<Result<ICollection<RentalCopy>>> ValidateCopiesForRentalAsync(int subscriberId, int? excludeRentalId, IEnumerable<int> serialNumbers, int? rentalId, CancellationToken ct = default);
    Task<Result<bool>> CanExtendRentalAsync(int rentalId, CancellationToken ct = default);
    Task<Result<int>> CalculateReturnPenaltyAsync(int rentalId, CancellationToken ct = default);

}
