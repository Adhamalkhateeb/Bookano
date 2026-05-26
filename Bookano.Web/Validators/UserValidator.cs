namespace Bookano.Web.Validators
{
    public class UserValidator : AbstractValidator<UserFormViewModel>
    {
        public UserValidator()
        {
            RuleFor(x => x.FullName)
                .MaximumLength(100)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.CharactersOnly_Eng)
                .WithMessage(Error.OnlyEnglishLetters);

            RuleFor(x => x.UserName)
                .MaximumLength(50)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.Username)
                .WithMessage(Error.InvalidUsername);

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Invalid Email")
                .MaximumLength(150)
                .WithMessage(Error.MaxLength);

            RuleFor(x => x.Password)
                .Length(8, 100)
                .WithMessage(Error.MaxMinLength)
                .Matches(RegexPatterns.Password)
                .WithMessage(Error.WeakPassword);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage(Error.PasswordNotMatch);
        }
    }
}
