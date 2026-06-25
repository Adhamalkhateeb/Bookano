using Bookano.Application.DTOs.BookCopies;
using Bookano.Application.DTOs.Dashboard;
using Bookano.Application.DTOs.Rentals;

namespace Bookano.Application.Services.Rentals;

public interface IRentalService
{
    Task<IEnumerable<RentalDto>> GetBySubscriberAsync(int subscriberId, CancellationToken ct = default);
    Task<IEnumerable<ChartItemDto>> GetRentalsPerDayAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);

    Task<RentalDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<RentalCopyDto>?> GetCopyRentalHistoryAsync(int copyId, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(RentalSaveDto dto, CancellationToken ct = default);
    Task<Result<int>> UpdateAsync(RentalSaveDto dto, CancellationToken ct = default);
    Task<Result<int>> ReturnAsync(RentalReturnDto dto, CancellationToken ct = default);
    Task<Result<int>> CancelAsync(int id, CancellationToken ct = default);
    Task<Result<BookCopyDto>> GetCopyReadyForRentalAsync(string value, CancellationToken ct = default);
    Task<int> GetTotalRentedCopiesAsync(CancellationToken ct = default);
}