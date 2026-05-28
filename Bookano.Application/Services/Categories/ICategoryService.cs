using Bookano.Application.DTOs.Categories;

namespace Bookano.Application.Services.Categories;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken ct = default);
    Task<CategoryDto?> GetByIdAsync(int id,CancellationToken ct = default);
    Task<CategoryDto?> AddAsync(CategoryFormDto formDto ,CancellationToken ct = default);
    Task<CategoryDto?> UpdateAsync(int id, CategoryFormDto formDto ,CancellationToken ct = default);
    Task<CategoryDto?> ToggleAsync(int id, CancellationToken ct = default);
    Task<bool> IsCategoryAllowedAsync(CategoryFormDto formDto, CancellationToken ct = default);
}