using System.Linq.Expressions;

namespace Bookano.Application.Validators.Identity;

public class ChangePasswordInputValidator<T> : AbstractValidator<T>
{
    protected ChangePasswordInputValidator(
        Expression<Func<T, string>> currentPasswordExpression,
        Expression<Func<T, string>> newPasswordExpression,
        Expression<Func<T, string>> confirmPasswordExpression
    )
    {
        RuleFor(currentPasswordExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField);

        RuleFor(newPasswordExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .Length(8, 100)
            .WithMessage(Error.MaxMinLength)
            .Matches(RegexPatterns.Password)
            .WithMessage(Error.WeakPassword);

        RuleFor(confirmPasswordExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .Equal(newPasswordExpression)
            .WithMessage(Error.PasswordNotMatch);
    }
}
