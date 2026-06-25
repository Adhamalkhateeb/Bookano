using Bookano.Application.DTOs.Areas;

namespace Bookano.Application.Services.Areas;

internal class AreaService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<AreaSaveDto> validator) : IAreaService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<AreaSaveDto> _validator = validator;

    public async Task<IEnumerable<AreaDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Areas.GetQueryable(withTracking: false)
            .ProjectTo<AreaDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<AreaDto>> GetGovernorateAreasAsync(int governorateId,CancellationToken ct = default)
    {
        return await _unitOfWork
           .Areas.GetQueryable(withTracking: false)
           .Where(x => x.GovernorateId == governorateId && !x.IsDeleted)
           .ProjectTo<AreaDto>(_mapper.ConfigurationProvider)
           .ToListAsync(ct);

    }

    public async Task<AreaDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Areas.GetQueryable(withTracking: false)
            .Where(x => x.Id == id)
            .ProjectTo<AreaDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<AreaDto>> AddAsync(AreaSaveDto dto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<AreaDto>.Failure(validationResult.ToValidationErrors());

        if(!await IsAreaAvailableAsync(dto.Name,dto.GovernorateId,-1,ct))
            return Result<AreaDto>.Failure("Area already exists.");

        var area = _mapper.Map<Area>(dto);

        _unitOfWork.Areas.Add(area);
        await _unitOfWork.SaveChangesAsync(ct);

        var dtoResult = await GetByIdAsync(area.Id, ct);

        return Result<AreaDto>.Success(dtoResult!);

    } 

    public async Task<Result<AreaDto>> UpdateAsync(
        int id,
        AreaSaveDto dto,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<AreaDto>.Failure(validationResult.ToValidationErrors());

        var area = await _unitOfWork.Areas.GetByIdAsync(id, ct);

        if (area is null)
            return Result<AreaDto>.Failure("Area not found.");

        if (!await IsAreaAvailableAsync(dto.Name, dto.GovernorateId,excludedId: id, ct))
            return Result<AreaDto>.Failure("Area already exists.");

        _mapper.Map(dto, area);

        await _unitOfWork.SaveChangesAsync(ct);
        var dtoResult = await GetByIdAsync(id, ct);
        return Result<AreaDto>.Success(dtoResult!);
    }

    public async Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default)
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(id, ct);

        if (area is null)
            return Result<ToggleStatusResult>.Failure("Area not found.");

        area.IsDeleted = !area.IsDeleted;

        await _unitOfWork.SaveChangesAsync(ct);

        return new ToggleStatusResult(area.IsDeleted, area.LastUpdatedOnUtc);
    }

    public async Task<bool> IsAreaAvailableAsync(
        string name,
        int governorateId,
        int excludedId,
        CancellationToken ct = default
    )
    {
        return !await _unitOfWork
            .Areas.IsExistsAsync(x => x.Name == name && x.GovernorateId == governorateId && x.Id != excludedId, ct);
    }


    
}
