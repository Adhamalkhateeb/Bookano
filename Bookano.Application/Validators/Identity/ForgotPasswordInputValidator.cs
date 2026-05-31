using System.Linq.Expressions;

namespace Bookano.Application.Validators.Identity;

public class ForgotPasswordInputValidator<T> : AbstractValidator<T>
{
    protected ForgotPasswordInputValidator(Expression<Func<T, string>> emailExpression)
    {
        RuleFor(emailExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .EmailAddress()
            .WithMessage("Invalid email address.")
            .MaximumLength(150)
            .WithMessage(Error.MaxLength);
    }
}
