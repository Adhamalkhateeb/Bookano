using Bookano.Application.Common.Models;
using Bookano.Application.DTOs.Users;

namespace Bookano.Application.Services.Users;

public interface IUserService
{
    Task<DataTableResult<UserDto>> GetPagedAsync(DataTableRequest request, CancellationToken ct = default);
    Task<UserFormDto?> GetUserFormAsync(string id, CancellationToken ct = default);
    Task<Result<string>> CreateAsync(UserFormDto dto, Func<string, string, string> callbackUrlProvider, CancellationToken ct = default);
    Task<Result<string>> UpdateAsync(UserFormDto dto, CancellationToken ct = default);
    Task<Result<string>> ToggleStatusAsync(string id, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(UserResetPasswordDto dto, CancellationToken ct = default);
    Task<Result> UnlockAsync(string id, CancellationToken ct = default);
    Task<bool> IsUserNameUniqueAsync(string userName, string? excludeId = null, CancellationToken ct = default);
    Task<bool> IsEmailUniqueAsync(string email, string? excludeId = null, CancellationToken ct = default);
    Task<IList<string>> GetRolesAsync(CancellationToken ct = default);
}
