namespace Bookano.Web.Core.ViewModels
{
    public class ResetPasswordFormViewModel
    {
        public string Id { get; set; } = null!;

        [
            DataType(DataType.Password),
            StringLength(100, ErrorMessage = Error.MaxMinLength, MinimumLength = 8),
            RegularExpression(RegexPatterns.Password, ErrorMessage = Error.WeakPassword)
        ]
        public string Password { get; set; } = null!;

        [
            DataType(DataType.Password),
            Display(Name = "Confirm password"),
            Compare("Password", ErrorMessage = Error.PasswordNotMatch),
        ]
        public string ConfirmPassword { get; set; } = null!;
    }
}
