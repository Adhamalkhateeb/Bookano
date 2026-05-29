using Bookano.Application.DTOs.Publishers;


namespace Bookano.Application.Services.Publishers;

internal class PublisherService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<PublisherFormDto> validator) : IPublisherService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<PublisherFormDto> _validator = validator;

    public async Task<IEnumerable<PublisherDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Publishers.GetQueryable()
            .ProjectTo<PublisherDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<PublisherDto>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Publishers.GetQueryable()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ProjectTo<PublisherDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<PublisherDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Publishers.GetQueryable()
            .ProjectTo<PublisherDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Result<PublisherDto>> AddAsync(PublisherFormDto formDto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(formDto, ct);
        if (!validationResult.IsValid)
            return Result<PublisherDto>.Failure(validationResult.ToValidationErrors());

        var publisher = _mapper.Map<Publisher>(formDto);

        _unitOfWork.Publishers.Add(publisher);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<PublisherDto>.Success(_mapper.Map<PublisherDto>(publisher));
    }

    public async Task<Result<PublisherDto>> UpdateAsync(int id, PublisherFormDto formDto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(formDto, ct);
        if (!validationResult.IsValid)
            return Result<PublisherDto>.Failure(validationResult.ToValidationErrors());

        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id, ct);

        if (publisher is null)
            return Result<PublisherDto>.Failure("Publisher not found.");

        _mapper.Map(formDto, publisher);

        _unitOfWork.Publishers.Update(publisher);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<PublisherDto>.Success(_mapper.Map<PublisherDto>(publisher));
    }

    public async Task<DateTimeOffset?> ToggleAsync(int id, CancellationToken ct = default)
    {
        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id, ct);

        if (publisher is null)
            return null;

        publisher.IsDeleted = !publisher.IsDeleted;

        _unitOfWork.Publishers.Update(publisher);
        await _unitOfWork.SaveChangesAsync(ct);

        return publisher.LastUpdatedOnUtc;
    }

    public async Task<bool> IsPublisherAllowedAsync(PublisherFormDto formDto, CancellationToken ct = default)
    {
        return !await _unitOfWork
            .Publishers.GetQueryable()
            .AnyAsync(p => p.Name == formDto.Name && p.Id != formDto.Id, ct);
    }

   
}