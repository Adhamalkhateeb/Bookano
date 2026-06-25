using Bookano.Application.Common.Models;
using Bookano.Application.DTOs.Users;

namespace Bookano.Application.Services.Users;

public interface IUserService
{
    Task<DataGridResult<UserDto>> GetPagedAsync(PaginationFilterQuery request, CancellationToken ct = default);
    Task<UserSaveDto?> GetUserForEditAsync(string id, CancellationToken ct = default);
    Task<Result<string>> CreateAsync(UserSaveDto dto, Func<string, string, string> callbackUrlProvider, CancellationToken ct = default);
    Task<Result<string>> UpdateAsync(UserSaveDto dto, CancellationToken ct = default);
    Task<Result<string>> ToggleStatusAsync(string id, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(UserResetPasswordDto dto, CancellationToken ct = default);
    Task<Result> UnlockAsync(string id, CancellationToken ct = default);
    Task<bool> IsUserNameUniqueAsync(string userName, string? excludeId = null, CancellationToken ct = default);
    Task<bool> IsEmailUniqueAsync(string email, string? excludeId = null, CancellationToken ct = default);
    Task<IList<string>> GetRolesAsync(CancellationToken ct = default);
}
