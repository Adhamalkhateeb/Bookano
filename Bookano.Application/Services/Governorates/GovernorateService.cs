using Bookano.Application.DTOs.Governorates;

namespace Bookano.Application.Services.Governorates;

internal class GovernorateService(IUnitOfWork unitOfWork, IMapper mapper) : IGovernorateService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<GovernorateDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork.Governorates.GetQueryable()
            .ProjectTo<GovernorateDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
