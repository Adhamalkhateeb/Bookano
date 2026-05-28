using Bookano.Application.DTOs.Governorates;

namespace Bookano.Application.Services.Governorates;

public interface IGovernorateService
{
    Task<IEnumerable<GovernorateDto>> GetAllAsync(CancellationToken ct = default);
}