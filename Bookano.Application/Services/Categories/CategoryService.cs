using Bookano.Application.DTOs.Categories;


namespace Bookano.Application.Services.Categories;

internal class CategoryService(IUnitOfWork unitOfWork, IMapper mapper) : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Categories.GetQueryable()
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

    public async Task<CategoryDto?> AddAsync(CategoryFormDto formDto, CancellationToken ct = default)
    {
        var category = _mapper.Map<Category>(formDto);

        _unitOfWork.Categories.Add(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, CategoryFormDto formDto, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);

        if (category is null)
            return null;

        _mapper.Map(formDto, category);

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto?> ToggleAsync(int id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);

        if (category is null)
            return null;

        category.IsDeleted = !category.IsDeleted;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> IsCategoryAllowedAsync(CategoryFormDto formDto, CancellationToken ct = default)
    {
        return !await _unitOfWork
            .Categories.GetQueryable()
            .AnyAsync(x => x.Name == formDto.Name && x.Id != formDto.Id, ct);
    }
}
