using Bookano.Application.DTOs.Categories;

namespace Bookano.Application.Services.Categories;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<CategoryDto>> GetAllActiveAsync(CancellationToken ct = default);
    Task<CategoryDto?> GetByIdAsync(int id,CancellationToken ct = default);
    Task<Result<CategoryDto>> AddAsync(CategoryFormDto formDto ,CancellationToken ct = default);
    Task<Result<CategoryDto>> UpdateAsync(int id, CategoryFormDto formDto ,CancellationToken ct = default);
    Task<DateTimeOffset?> ToggleAsync(int id, CancellationToken ct = default);
    Task<bool> IsCategoryAllowedAsync(CategoryFormDto formDto, CancellationToken ct = default);
}