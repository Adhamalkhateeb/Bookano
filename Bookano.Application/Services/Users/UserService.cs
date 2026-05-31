using Bookano.Application.Common;
using Bookano.Application.Common.Interfaces;
using Bookano.Application.DTOs.Users;
using Microsoft.AspNetCore.Identity;

namespace Bookano.Application.Services.Users;

public sealed class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IUserNotificationService userNotificationService,
    IMapper mapper,
    DataTableQueryBuilder<ApplicationUser> builder,
    IValidator<UserFormDto> validator,
    IValidator<UserResetPasswordDto> resetPasswordValidator
) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IUserNotificationService _userNotificationService = userNotificationService;
    private readonly IMapper _mapper = mapper;
    private readonly DataTableQueryBuilder<ApplicationUser> _builder = builder;
    private readonly IValidator<UserFormDto> _validator = validator;
    private readonly IValidator<UserResetPasswordDto> _resetPasswordValidator = resetPasswordValidator;

    private static readonly List<string> AllowedSortColumns =
        [
            "Id", "FullName", "UserName", "Email", "IsDeleted", "CreatedOnUtc", "LastUpdatedOnUtc"
        ];

    public async Task<DataTableResult<UserDto>> GetPagedAsync(
        DataTableRequest request,
        CancellationToken ct = default)
    {
        var query = _userManager.Users.AsQueryable();

        return await _builder.For(query)
            .WithRequest(request)
            .AllowSorting([.. AllowedSortColumns])
            .Search((q, s) =>
            {
                return q.Where(b =>
                    b.UserName!.Contains(s) || b.Email!.Contains(s)
                );
            })
            .Sort()
            .ExecuteAsync<UserDto>(ct);
    }

    public async Task<UserFormDto?> GetUserFormAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return null;

        var dto = _mapper.Map<UserFormDto>(user);
        dto.SelectedRoles = await _userManager.GetRolesAsync(user);
        return dto;
    }

    public async Task<Result<string>> CreateAsync(
        UserFormDto dto,
        Func<string, string, string> callbackUrlProvider,
        CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<string>.Failure(validationResult.ToValidationErrors());

        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            UserName = dto.UserName,
            Email = dto.Email,
        };
        var result = await _userManager.CreateAsync(user, dto.Password!);

        if (result.Succeeded)
        {
            await _userManager.AddToRolesAsync(user, dto.SelectedRoles);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(code));

            var callbackUrl = callbackUrlProvider(user.Id, code);

            await _userNotificationService.SendWelcomeEmailAsync(user.Email!, user.FullName, callbackUrl);

            return Result<string>.Success(user.Id);
        }

        return Result<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result<string>> UpdateAsync(UserFormDto dto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<string>.Failure(validationResult.ToValidationErrors());

        var user = await _userManager.FindByIdAsync(dto.Id!);
        if (user is null)
            return Result<string>.Failure("User not found.");

        _mapper.Map(dto, user);

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesUpdated = !currentRoles.SequenceEqual(dto.SelectedRoles);

            if (rolesUpdated)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRolesAsync(user, dto.SelectedRoles);
            }

            await _userManager.UpdateSecurityStampAsync(user);

            return Result<string>.Success(user.Id);
        }

        return Result<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result<string>> ToggleStatusAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result<string>.Failure("User not found.");

        user.IsDeleted = !user.IsDeleted;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (user.IsDeleted)
            await _userManager.UpdateSecurityStampAsync(user);

        return Result<string>.Success(user.LastUpdatedOnUtc?.ToString() ?? string.Empty);
    }

    public async Task<Result> ResetPasswordAsync(UserResetPasswordDto dto, CancellationToken ct = default)
    {
        var validationResult = await _resetPasswordValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result.Failure(validationResult.ToValidationErrors());

        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user is null)
            return Result.Failure("User not found.");

        var currentPasswordHash = user.PasswordHash;

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return Result.Failure(string.Join(", ", removeResult.Errors.Select(e => e.Description)));

        var addResult = await _userManager.AddPasswordAsync(user, dto.Password);
        if (addResult.Succeeded)
        {
            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
                return Result.Success();

            return Result.Failure(string.Join(", ", updateResult.Errors.Select(e => e.Description)));
        }

        user.PasswordHash = currentPasswordHash;
        await _userManager.UpdateAsync(user);

        return Result.Failure(string.Join(", ", addResult.Errors.Select(e => e.Description)));
    }

    public async Task<Result> UnlockAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result.Failure("User not found.");

        var isLockedOut = await _userManager.IsLockedOutAsync(user);
        if (isLockedOut)
        {
            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (!result.Succeeded)
                return Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return Result.Success();
    }

    public async Task<bool> IsUserNameUniqueAsync(string userName, string? excludeId = null, CancellationToken ct = default)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user is null || user.Id.Equals(excludeId);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, string? excludeId = null, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null || user.Id.Equals(excludeId);
    }

    public async Task<IList<string>> GetRolesAsync(CancellationToken ct = default)
    {
        return await _roleManager.Roles.Select(r => r.Name!).ToListAsync(ct);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
