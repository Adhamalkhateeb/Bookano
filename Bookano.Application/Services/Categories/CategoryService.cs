using Bookano.Application.DTOs.Categories;

namespace Bookano.Application.Services.Categories;

internal class CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CategoryFormDto> validator) : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<CategoryFormDto> _validator = validator;

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Categories.GetQueryable()
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllActiveAsync(CancellationToken ct = default)
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
            .Categories.GetQueryable()
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Result<CategoryDto>> AddAsync(CategoryFormDto formDto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(formDto, ct);
        if (!validationResult.IsValid)
            return Result<CategoryDto>.Failure(validationResult.ToValidationErrors());

        var category = _mapper.Map<Category>(formDto);

        _unitOfWork.Categories.Add(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
    }

    public async Task<Result<CategoryDto>> UpdateAsync(int id, CategoryFormDto formDto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(formDto, ct);
        if (!validationResult.IsValid)
            return Result<CategoryDto>.Failure(validationResult.ToValidationErrors());

        var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);

        if (category is null)
            return Result<CategoryDto>.Failure("Category not found.");

        _mapper.Map(formDto, category);

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
    }

    public async Task<DateTimeOffset?> ToggleAsync(int id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);

        if (category is null)
            return null;

        category.IsDeleted = !category.IsDeleted;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return category.LastUpdatedOnUtc;
    }

    public async Task<bool> IsCategoryAllowedAsync(CategoryFormDto formDto, CancellationToken ct = default)
    {
        return !await _unitOfWork
            .Categories.GetQueryable()
            .AnyAsync(x => x.Name == formDto.Name && x.Id != formDto.Id, ct);
    }


}
