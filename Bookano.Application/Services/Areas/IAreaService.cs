

using Bookano.Application.DTOs.Areas;

namespace Bookano.Application.Services.Areas;

public interface IAreaService
{
    Task<IEnumerable<AreaDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<AreaDto>> GetGovernorateAreasAsync(int governorateId,CancellationToken ct = default);
    Task<AreaDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<AreaDto>> AddAsync(AreaSaveDto area, CancellationToken ct = default);
    Task<Result<AreaDto>> UpdateAsync(int id, AreaSaveDto area, CancellationToken ct = default);
    Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default);
    Task<bool> IsAreaAvailableAsync(string name,int governorateId, int excludedId, CancellationToken ct = default);
}
