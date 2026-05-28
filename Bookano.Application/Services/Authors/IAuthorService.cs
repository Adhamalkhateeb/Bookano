using Bookano.Application.DTOs.Authors;

namespace Bookano.Application.Services.Authors;

public interface IAuthorService
{
    Task<IEnumerable<AuthorDto>> GetAllAsync(CancellationToken ct = default);
    Task<AuthorDto?> GetAsync(int id, CancellationToken ct = default);
    Task<AuthorDto?> AddAsync(AuthorFormDto author, CancellationToken ct = default);
    Task<AuthorDto?> UpdateAsync(int id, AuthorFormDto author, CancellationToken ct = default);
    Task<AuthorDto?> ToggleAsync(int id, CancellationToken ct = default);
    Task<bool> IsNameAvailableAsync(int id, string name, CancellationToken ct = default);
}
