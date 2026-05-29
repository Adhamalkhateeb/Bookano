using Bookano.Application.DTOs.Areas;

namespace Bookano.Application.Services.Areas;

public interface IGovernorateService
{
    Task<IEnumerable<GovernorateDto>> GetAllAsync(CancellationToken ct = default);
}