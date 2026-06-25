using Bookano.Application.DTOs.Categories;

namespace Bookano.Application.Services.Categories;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<CategoryDto>> GetActiveAsync(CancellationToken ct = default);
    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<CategoryDto>> AddAsync(CategorySaveDto dto, CancellationToken ct = default);
    Task<Result<CategoryDto>> UpdateAsync(int id, CategorySaveDto dto, CancellationToken ct = default);
    Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default);
    Task<bool> IsNameAvailableAsync(string name, int excludedId, CancellationToken ct = default);
}