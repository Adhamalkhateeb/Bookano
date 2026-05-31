using Bookano.Application.DTOs.Rentals;

namespace Bookano.Application.Services.Rentals;

public interface IRentalService
{
    Task<Result<int>> GetAvailableCopiesCountAsync(int subscriberId, int? excludeRentalId = null, CancellationToken ct = default);

    Task<Result<RentalReturnDto>> GetReturnFormAsync(int id, CancellationToken ct = default);

    Task<RentalDto?> GetDetailsAsync(int id, CancellationToken ct = default);

    Task<IList<RentalBookCopyDto>> GetCopiesForDisplayAsync(
        IEnumerable<int> serialNumbers,
        CancellationToken ct = default
    );

    Task<Result<RentalBookCopyDto>> GetCopyDetailsAsync(
        string value,
        CancellationToken ct = default
    );

    Task<Result<int>> CreateAsync(RentalFormDto dto, CancellationToken ct = default);

    Task<Result<int>> UpdateAsync(RentalFormDto dto, CancellationToken ct = default);

    Task<Result<int>> ReturnAsync(RentalReturnDto dto, CancellationToken ct = default);

    Task<Result<int>> CancelAsync(int id, CancellationToken ct = default);
}