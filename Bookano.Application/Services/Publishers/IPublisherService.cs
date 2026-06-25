using Bookano.Application.DTOs.Publishers;

namespace Bookano.Application.Services.Publishers;

public interface IPublisherService
{
    Task<IEnumerable<PublisherDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<PublisherDto>> GetActiveAsync(CancellationToken ct = default);
    Task<PublisherDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<PublisherDto>> AddAsync(PublisherSaveDto dto, CancellationToken ct = default);
    Task<Result<PublisherDto>> UpdateAsync(int id, PublisherSaveDto dto, CancellationToken ct = default);
    Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default);
    Task<bool> IsNameAvailableAsync(string name, int excludedId, CancellationToken ct = default);
}