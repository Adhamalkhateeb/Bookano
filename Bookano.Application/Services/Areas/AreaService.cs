

using Bookano.Application.DTOs.Areas;

namespace Bookano.Application.Services.Areas;

internal class AreaService(IUnitOfWork unitOfWork, IMapper mapper) : IAreaService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<AreaDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Areas.GetQueryable()
            .ProjectTo<AreaDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<AreaDto?> GetAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Areas.GetQueryable()
            .ProjectTo<AreaDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<AreaDto?> AddAsync(AreaFormDto areaDto, CancellationToken ct = default)
    {
        var area = _mapper.Map<Area>(areaDto);

        _unitOfWork.Areas.Add(area);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetAsync(area.Id, ct);
    }

    public async Task<AreaDto?> UpdateAsync(
        int id,
        AreaFormDto dto,
        CancellationToken ct = default
    )
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(id, ct);

        if (area is null)
            return null;

        _mapper.Map(dto, area);

        _unitOfWork.Areas.Update(area);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetAsync(id, ct);
    }

    public async Task<AreaDto?> ToggleAsync(int id, CancellationToken ct = default)
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(id, ct);

        if (area is null)
            return null;

        area.IsDeleted = !area.IsDeleted;

        _unitOfWork.Areas.Update(area);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<AreaDto>(area);
    }

    public async Task<bool> IsAreaAvailableAsync(
        int id,
        int governorateId,
        string name,
        CancellationToken ct = default
    )
    {
        return !await _unitOfWork
            .Areas.GetQueryable()
            .AnyAsync(x => x.Name == name && x.GovernorateId == governorateId && x.Id != id, ct);
    }
}
