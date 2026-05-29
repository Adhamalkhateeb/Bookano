using Bookano.Application.DTOs.Authors;

namespace Bookano.Application.Services.Authors;

internal class AuthorService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<AuthorFormDto> validator) : IAuthorService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<AuthorFormDto> _validator = validator;

    public async Task<IEnumerable<AuthorDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Authors.GetQueryable()
            .ProjectTo<AuthorDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<AuthorDto>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Authors.GetQueryable()
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Name)
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

    public async Task<Result<AuthorDto>> AddAsync(AuthorFormDto authorDto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(authorDto, ct);
        if (!validationResult.IsValid)
            return Result<AuthorDto>.Failure(validationResult.ToValidationErrors());

        var author = _mapper.Map<Author>(authorDto);

        _unitOfWork.Authors.Add(author);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AuthorDto>.Success(_mapper.Map<AuthorDto>(author));
    }

    public async Task<Result<AuthorDto>> UpdateAsync(
        int id,
        AuthorFormDto dto,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<AuthorDto>.Failure(validationResult.ToValidationErrors());

        var author = await _unitOfWork.Authors.GetByIdAsync(id, ct);

        if (author is null)
            return Result<AuthorDto>.Failure("Author not found.");

        _mapper.Map(dto, author);

        _unitOfWork.Authors.Update(author);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AuthorDto>.Success(_mapper.Map<AuthorDto>(author));
    }

    public async Task<DateTimeOffset?> ToggleAsync(int id, CancellationToken ct = default)
    {
        var author = await _unitOfWork.Authors.GetByIdAsync(id, ct);

        if (author is null)
            return null;

        author.IsDeleted = !author.IsDeleted;

        _unitOfWork.Authors.Update(author);
        await _unitOfWork.SaveChangesAsync(ct);

        return author.LastUpdatedOnUtc;
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
