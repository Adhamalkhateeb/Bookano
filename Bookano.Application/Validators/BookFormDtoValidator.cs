using Bookano.Application.DTOs.Books;
using System.Linq.Expressions;

namespace Bookano.Application.Validators;


public class BookCommonValidator<T> : AbstractValidator<T>
{
    public BookCommonValidator(
         Expression<Func<T, string>> isbnExpression,
         Expression<Func<T, string>> titleExpression,
         Expression<Func<T, string>> hallExpression)
    {

        RuleFor(isbnExpression).MaximumLength(20).WithMessage(Error.MaxLength);

        RuleFor(titleExpression).NotEmpty().MaximumLength(200).WithMessage(Error.MaxLength);

        RuleFor(hallExpression).NotEmpty().MaximumLength(50).WithMessage(Error.MaxLength);
    }
}

public class BookFormDtoValidator : BookCommonValidator<BookFormDto>
{
    public BookFormDtoValidator() : base(x => x.Isbn!, x => x.Title, x => x.Hall)
    {

        RuleFor(x => x.PublisherId).NotEmpty().WithMessage(Error.RequiredField);

        RuleFor(x => x.PublishingDate).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today)).WithMessage(Error.NotAllowFutureDates);

        RuleFor(x => x.Description).NotEmpty();

        RuleFor(x => x.SelectedAuthors).NotEmpty();

        RuleFor(x => x.SelectedCategories).NotEmpty();

    }
}
