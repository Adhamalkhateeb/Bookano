using Bookano.Application.DTOs.Authors;

namespace Bookano.Application.Services.Authors;

public interface IAuthorService
{
    Task<IEnumerable<AuthorDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<AuthorDto>> GetActiveAsync(CancellationToken ct = default);
    Task<AuthorDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<AuthorDto>> AddAsync(AuthorSaveDto author, CancellationToken ct = default);
    Task<Result<AuthorDto>> UpdateAsync(int id, AuthorSaveDto author, CancellationToken ct = default);
    Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default);
    Task<bool> IsNameAvailableAsync(string name, int excludedId, CancellationToken ct = default);
}
