
using Bookano.Application.DTOs.BookCopies;
using System.Linq.Expressions;

namespace Bookano.Application.Validators;

public class BookCopyCommonValidator<T> : AbstractValidator<T> {
    public BookCopyCommonValidator(
        Expression<Func<T,int>> editionNumberExpression)
    {
        RuleFor(editionNumberExpression)
               .InclusiveBetween(1, 1000)
               .WithMessage(Error.ShouldBeInRange);
    }
}

public class BookCopyFormDtoValidator : BookCopyCommonValidator<BookCopyFormDto>
{
    public BookCopyFormDtoValidator() : base(x => x.EditionNumber) {}
}
