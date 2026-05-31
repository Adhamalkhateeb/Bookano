using System.Linq.Expressions;

namespace Bookano.Application.Validators.Identity;

public class ManageEmailInputValidator<T> : AbstractValidator<T>
{
    protected ManageEmailInputValidator(Expression<Func<T, string>> newEmailExpression)
    {
        RuleFor(newEmailExpression)
            .NotEmpty()
            .WithMessage(Error.RequiredField)
            .EmailAddress()
            .WithMessage("Invalid email address.")
            .MaximumLength(150)
            .WithMessage(Error.MaxLength);
    }
}
