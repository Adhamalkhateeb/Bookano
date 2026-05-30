

using Bookano.Application.DTOs.Areas;

namespace Bookano.Application.Services.Areas;

public interface IAreaService
{
    Task<IEnumerable<AreaDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<AreaDto>> GetGovernorateAreasAsync(int governorateId,CancellationToken ct = default);
    Task<AreaDto?> GetAsync(int id, CancellationToken ct = default);
    Task<Result<AreaDto>> AddAsync(AreaFormDto area, CancellationToken ct = default);
    Task<Result<AreaDto>> UpdateAsync(int id, AreaFormDto area, CancellationToken ct = default);
    Task<DateTimeOffset?> ToggleAsync(int id, CancellationToken ct = default);
    Task<bool> IsAreaAvailableAsync(int id,int governorateId, string name, CancellationToken ct = default);
}
