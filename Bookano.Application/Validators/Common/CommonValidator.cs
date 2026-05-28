using System.Linq.Expressions;

namespace Bookano.Application.Validators.Common;

public abstract class CommonValidator<T> : AbstractValidator<T>
{
        protected CommonValidator(
            Expression<Func<T, string>> nameExpression)
        {
            RuleFor(nameExpression)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.CharactersOnly_Eng)
                .WithMessage(Error.OnlyEnglishLetters);
        }
    
}
