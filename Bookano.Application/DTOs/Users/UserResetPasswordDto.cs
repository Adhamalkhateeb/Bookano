namespace Bookano.Application.DTOs.Users;

public sealed class UserResetPasswordDto
{
    public string Id { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}
