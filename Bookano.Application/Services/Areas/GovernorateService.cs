using Bookano.Application.DTOs.Areas;

namespace Bookano.Application.Services.Areas;

internal class GovernorateService(IUnitOfWork unitOfWork, IMapper mapper) : IGovernorateService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<GovernorateDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork.Governorates.GetQueryable()
            .OrderBy(g => g.Name)
            .ProjectTo<GovernorateDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
