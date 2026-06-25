using Bookano.Application.DTOs.Authors;

namespace Bookano.Application.Services.Authors;

internal class AuthorService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<AuthorSaveDto> validator) : IAuthorService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<AuthorSaveDto> _validator = validator;

    public async Task<IEnumerable<AuthorDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Authors.GetQueryable(withTracking: false)
            .ProjectTo<AuthorDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<AuthorDto>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Authors.GetQueryable(withTracking: false)
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Name)
            .ProjectTo<AuthorDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<AuthorDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Authors.GetQueryable(withTracking: false)
            .Where(a => a.Id == id)
            .ProjectTo<AuthorDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<AuthorDto>> AddAsync(AuthorSaveDto dto, CancellationToken ct = default)
    {

        var validationResult = await ValidateAuthorSaveAsync(0, dto, ct);

        if (validationResult.IsFailure)
            return validationResult;

        var author = _mapper.Map<Author>(dto);

        _unitOfWork.Authors.Add(author);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<AuthorDto>(author);
    }

    public async Task<Result<AuthorDto>> UpdateAsync(
        int id,
        AuthorSaveDto dto,
        CancellationToken ct = default
    )
    {

        var validationResult = await ValidateAuthorSaveAsync(id, dto, ct);

        if (validationResult.IsFailure)
            return validationResult;

        var author = await _unitOfWork.Authors.GetByIdAsync(id, ct);

        if (author is null)
            return Result<AuthorDto>.Failure("Author not found.");

        _mapper.Map(dto, author);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<AuthorDto>(author);
    }

    public async Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default)
    {
        var author = await _unitOfWork.Authors.GetByIdAsync(id, ct);

        if (author is null)
            return Result<ToggleStatusResult>.Failure("Author not found.");

        author.IsDeleted = !author.IsDeleted;

        await _unitOfWork.SaveChangesAsync(ct);

        return new ToggleStatusResult(author.IsDeleted,author.LastUpdatedOnUtc);
    }

    public async Task<bool> IsNameAvailableAsync(
        string name,
        int excludedId,
        CancellationToken ct = default
    )
    {
        return !await _unitOfWork.Authors.IsExistsAsync(x => x.Name == name && x.Id != excludedId, ct);
    }


    private async Task<Result<AuthorDto>> ValidateAuthorSaveAsync(int id ,AuthorSaveDto dto ,CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<AuthorDto>.Failure(validationResult.ToValidationErrors());

        if (!await IsNameAvailableAsync(name: dto.Name, excludedId: id, ct))
            return Result<AuthorDto>.Failure("Author name already exists.");

        return (Result<AuthorDto>) Result.Success();
    }




}
