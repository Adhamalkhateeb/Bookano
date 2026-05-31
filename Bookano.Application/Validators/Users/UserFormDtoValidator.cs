using Bookano.Application.DTOs.Users;
using System.Linq.Expressions;

namespace Bookano.Application.Validators.Users;

public class UserCommonValidator<T> : AbstractValidator<T>
{
    public UserCommonValidator(
        Expression<Func<T, string>> fullNameExpression,
        Expression<Func<T, string>> userNameExpression,
        Expression<Func<T, string>> emailExpression
    )
    {
        RuleFor(fullNameExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .MaximumLength(100)
            .WithMessage(Error.MaxLength)
            .Matches(RegexPatterns.CharactersOnly_Eng)
            .WithMessage(Error.OnlyEnglishLetters);

        RuleFor(userNameExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .MaximumLength(50)
            .WithMessage(Error.MaxLength)
            .Matches(RegexPatterns.Username)
            .WithMessage(Error.InvalidUsername);

        RuleFor(emailExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .EmailAddress()
            .WithMessage("Invalid Email")
            .MaximumLength(150)
            .WithMessage(Error.MaxLength);
    }
}


public sealed class UserFormDtoValidator : UserCommonValidator<UserFormDto>
{
    public UserFormDtoValidator()
        : base(x => x.FullName, x => x.UserName, x => x.Email)
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.Id))
            .WithMessage(Error.RequiredField)
            .Length(8, 100)
            .WithMessage(Error.MaxMinLength)
            .Matches(RegexPatterns.Password)
            .WithMessage(Error.WeakPassword)
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.Id) || !string.IsNullOrEmpty(x.Password))
            .WithMessage(Error.RequiredField)
            .Equal(x => x.Password)
            .WithMessage(Error.PasswordNotMatch);
    }
}
