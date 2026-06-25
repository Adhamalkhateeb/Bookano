using Bookano.Application.DTOs.Publishers;


namespace Bookano.Application.Services.Publishers;

internal class PublisherService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<PublisherSaveDto> validator) : IPublisherService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<PublisherSaveDto> _validator = validator;

    public async Task<IEnumerable<PublisherDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Publishers.GetQueryable(withTracking: false)
            .ProjectTo<PublisherDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<PublisherDto>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Publishers.GetQueryable(withTracking: false)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ProjectTo<PublisherDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<PublisherDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Publishers.GetQueryable(withTracking: false)
            .Where(p => p.Id == id)
            .ProjectTo<PublisherDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<PublisherDto>> AddAsync(PublisherSaveDto dto, CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(0, dto, ct);
        if (validationResult.IsFailure)
            return validationResult;

        var publisher = _mapper.Map<Publisher>(dto);

        _unitOfWork.Publishers.Add(publisher);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<PublisherDto>(publisher);
    }

    public async Task<Result<PublisherDto>> UpdateAsync(int id, PublisherSaveDto dto, CancellationToken ct = default)
    {

        var validationResult = await ValidateAsync(id, dto, ct);
        if (validationResult.IsFailure)
            return validationResult;

        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id, ct);

        if (publisher is null)
            return Result<PublisherDto>.Failure("Publisher not found.");

        _mapper.Map(dto, publisher);

        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<PublisherDto>(publisher);
    }

    public async Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default)
    {
        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id, ct);

        if (publisher is null)
            return Result<ToggleStatusResult>.Failure("Publisher not found.");

        publisher.IsDeleted = !publisher.IsDeleted;

        await _unitOfWork.SaveChangesAsync(ct);

        return new ToggleStatusResult(publisher.IsDeleted, publisher.LastUpdatedOnUtc);
    }

    public async Task<bool> IsNameAvailableAsync(string name, int excludedId, CancellationToken ct = default)
    {
        return !await _unitOfWork
            .Publishers.GetQueryable()
            .AnyAsync(p => p.Name == name && p.Id != excludedId, ct);
    }


    private async Task<Result<PublisherDto>> ValidateAsync(int id,PublisherSaveDto dto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<PublisherDto>.Failure(validationResult.ToValidationErrors());

        if (!await IsNameAvailableAsync(name: dto.Name, excludedId: id, ct))
            return Result<PublisherDto>.Failure("Publisher name already exists.");

        return (Result<PublisherDto>)Result.Success();
    }

   
}