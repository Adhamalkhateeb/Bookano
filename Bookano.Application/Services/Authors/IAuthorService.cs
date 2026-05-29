using Bookano.Application.DTOs.Authors;

namespace Bookano.Application.Services.Authors;

public interface IAuthorService
{
    Task<IEnumerable<AuthorDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<AuthorDto>> GetAllActiveAsync(CancellationToken ct = default);
    Task<AuthorDto?> GetAsync(int id, CancellationToken ct = default);
    Task<Result<AuthorDto>> AddAsync(AuthorFormDto author, CancellationToken ct = default);
    Task<Result<AuthorDto>> UpdateAsync(int id, AuthorFormDto author, CancellationToken ct = default);
    Task<DateTimeOffset?> ToggleAsync(int id, CancellationToken ct = default);
    Task<bool> IsNameAvailableAsync(int id, string name, CancellationToken ct = default);
}
