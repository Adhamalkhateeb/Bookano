namespace Bookano.Application.DTOs.Users;

public sealed class UserSaveDto
{
    public string? Id { get; set; }
    public string FullName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public IList<string> SelectedRoles { get; set; } = [];
}
