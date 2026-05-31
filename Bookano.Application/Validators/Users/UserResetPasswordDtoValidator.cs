using Bookano.Application.DTOs.Users;
using System.Linq.Expressions;

namespace Bookano.Application.Validators.Users;

public class ResetPasswordCommonValidator<T> : AbstractValidator<T>
{
    public ResetPasswordCommonValidator(
        Expression<Func<T, string>> passwordExpression,
        Expression<Func<T, string>> confirmPasswordExpression
    )
    {
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

public sealed class UserResetPasswordDtoValidator : ResetPasswordCommonValidator<UserResetPasswordDto>
{
    public UserResetPasswordDtoValidator()
        : base(x => x.Password, x => x.ConfirmPassword)
    {
    }
}
