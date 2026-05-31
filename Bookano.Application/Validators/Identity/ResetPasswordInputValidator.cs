using System.Linq.Expressions;

namespace Bookano.Application.Validators.Identity;

public class ResetPasswordInputValidator<T> : AbstractValidator<T>
{
    protected ResetPasswordInputValidator(
        Expression<Func<T, string>> emailExpression,
        Expression<Func<T, string>> passwordExpression,
        Expression<Func<T, string>> confirmPasswordExpression
    )
    {
        RuleFor(emailExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .EmailAddress()
            .WithMessage("Invalid email address.")
            .MaximumLength(150)
            .WithMessage(Error.MaxLength);

        RuleFor(passwordExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .Length(8, 100)
            .WithMessage(Error.MaxMinLength)
            .Matches(RegexPatterns.Password)
            .WithMessage(Error.WeakPassword);

        RuleFor(confirmPasswordExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .Equal(passwordExpression)
            .WithMessage(Error.PasswordNotMatch);
    }
}
