using Bookano.Application.DTOs.Publishers;


namespace Bookano.Application.Services.Publishers;

internal class PublisherService(IUnitOfWork unitOfWork, IMapper mapper) : IPublisherService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<PublisherDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Publishers.GetQueryable()
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

    public async Task<PublisherDto?> AddAsync(PublisherFormDto formDto, CancellationToken ct = default)
    {
        var publisher = _mapper.Map<Publisher>(formDto);

        _unitOfWork.Publishers.Add(publisher);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<PublisherDto>(publisher);
    }

    public async Task<PublisherDto?> UpdateAsync(int id, PublisherFormDto formDto, CancellationToken ct = default)
    {
        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id, ct);

        if (publisher is null)
            return null;

        _mapper.Map(formDto, publisher);

        _unitOfWork.Publishers.Update(publisher);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<PublisherDto>(publisher);
    }

    public async Task<PublisherDto?> ToggleAsync(int id, CancellationToken ct = default)
    {
        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id, ct);

        if (publisher is null)
            return null;

        publisher.IsDeleted = !publisher.IsDeleted;

        _unitOfWork.Publishers.Update(publisher);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<PublisherDto>(publisher);
    }

    public async Task<bool> IsPublisherAllowedAsync(PublisherFormDto formDto, CancellationToken ct = default)
    {
        return !await _unitOfWork
            .Publishers.GetQueryable()
            .AnyAsync(p => p.Name == formDto.Name && p.Id != formDto.Id, ct);
    }
}