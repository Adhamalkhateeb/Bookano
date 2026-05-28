using Bookano.Application.DTOs.Authors;

namespace Bookano.Application.Services.Authors;

internal class AuthorService(IUnitOfWork unitOfWork, IMapper mapper) : IAuthorService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<AuthorDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Authors.GetQueryable()
            .ProjectTo<AuthorDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<AuthorDto?> GetAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Authors.GetQueryable()
            .ProjectTo<AuthorDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<AuthorDto?> AddAsync(AuthorFormDto authorDto, CancellationToken ct = default)
    {
        var author = _mapper.Map<Author>(authorDto);

        _unitOfWork.Authors.Add(author);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<AuthorDto>(author);
    }

    public async Task<AuthorDto?> UpdateAsync(
        int id,
        AuthorFormDto dto,
        CancellationToken ct = default
    )
    {
        var author = await _unitOfWork.Authors.GetByIdAsync(id, ct);

        if (author is null)
            return null;

        _mapper.Map(dto, author);

        _unitOfWork.Authors.Update(author);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<AuthorDto>(author);
    }

    public async Task<AuthorDto?> ToggleAsync(int id, CancellationToken ct = default)
    {
        var author = await _unitOfWork.Authors.GetByIdAsync(id, ct);

        if (author is null)
            return null;

        author.IsDeleted = !author.IsDeleted;

        _unitOfWork.Authors.Update(author);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<AuthorDto>(author);
    }

    public async Task<bool> IsNameAvailableAsync(
        int id,
        string name,
        CancellationToken ct = default
    )
    {
        return !await _unitOfWork
            .Authors.GetQueryable()
            .AnyAsync(x => x.Name == name && x.Id != id, ct);
    }
}
