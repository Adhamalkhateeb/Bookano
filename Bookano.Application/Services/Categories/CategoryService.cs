using Bookano.Application.DTOs.Categories;

namespace Bookano.Application.Services.Categories;

internal class CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CategorySaveDto> validator) : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<CategorySaveDto> _validator = validator;

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Categories.GetQueryable()
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<CategoryDto>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
           .Categories.GetQueryable()
           .Where(x => !x.IsDeleted)
           .OrderBy(x => x.Name)
           .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
           .ToListAsync(ct);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Categories.GetQueryable(withTracking: false)
            .Where(c => c.Id == id)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<CategoryDto>> AddAsync(CategorySaveDto dto, CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(0, dto, ct);
        if (validationResult.IsFailure)
            return validationResult;

        var category = _mapper.Map<Category>(dto);

        _unitOfWork.Categories.Add(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<Result<CategoryDto>> UpdateAsync(int id, CategorySaveDto dto, CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(0, dto, ct);
        if (validationResult.IsFailure)
            return validationResult;

        var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);

        if (category is null)
            return Result<CategoryDto>.Failure("Category not found.");

        _mapper.Map(dto, category);

        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);

        if (category is null)
            return Result<ToggleStatusResult>.Failure("Category not found.");

        category.IsDeleted = !category.IsDeleted;

        await _unitOfWork.SaveChangesAsync(ct);

        return new ToggleStatusResult(category.IsDeleted, category.LastUpdatedOnUtc);
    }

    public async Task<bool> IsNameAvailableAsync(string name, int excludedId, CancellationToken ct = default)
    {
        return !await _unitOfWork
            .Categories.GetQueryable()
            .AnyAsync(x => x.Name == name && x.Id != excludedId, ct);
    }

    private async Task<Result<CategoryDto>> ValidateAsync(int id,CategorySaveDto dto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<CategoryDto>.Failure(validationResult.ToValidationErrors());

        if (!await IsNameAvailableAsync(name: dto.Name, excludedId: id, ct))
            return Result<CategoryDto>.Failure("Category name already exists.");

        return (Result<CategoryDto>)Result.Success();
    }


}
