using Bookano.Application.DTOs.Governorates;

namespace Bookano.Application.Services.Governorates;

public interface IGovernorateService
{
    Task<IEnumerable<GovernorateDto>> GetActiveAsync(CancellationToken ct = default);
}