namespace Bookano.Web.Validators
{
    public class ResetPasswordValidaor : AbstractValidator<ResetPasswordFormViewModel>
    {
        public ResetPasswordValidaor()
        {
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
