namespace Bookano.Web.Validators
{
    public class BookCopyValidator : AbstractValidator<BookCopyFormViewModel>
    {
        public BookCopyValidator()
        {
            RuleFor(x => x.EditionNumber)
                .InclusiveBetween(1, 1000)
                .WithMessage(Error.ShouldBeInRange);
        }
    }
}
