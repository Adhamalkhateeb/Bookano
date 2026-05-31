using System.Linq.Expressions;

namespace Bookano.Application.Validators.Identity;

public class ManageProfileInputValidator<T> : AbstractValidator<T>
{
    protected ManageProfileInputValidator(
        Expression<Func<T, string>> fullNameExpression,
        Expression<Func<T, string?>> phoneNumberExpression
    )
    {
        RuleFor(fullNameExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .MaximumLength(100)
            .WithMessage(Error.MaxLength)
            .Matches(RegexPatterns.CharactersOnly_Eng)
            .WithMessage(Error.OnlyEnglishLetters);

        RuleFor(phoneNumberExpression)
            .MaximumLength(11)
            .WithMessage(Error.MaxLength)
            .Matches(RegexPatterns.MobileNumber)
            .WithMessage(Error.InvalidMobileNumber)
            .When(x => !string.IsNullOrEmpty(phoneNumberExpression.Compile()(x)));
    }
}
