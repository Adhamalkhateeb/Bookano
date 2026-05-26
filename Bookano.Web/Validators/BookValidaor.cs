namespace Bookano.Web.Validators
{
    public class BookValidaor : AbstractValidator<BookFormViewModel>
    {
        public BookValidaor()
        {
            RuleFor(x => x.Isbn).MaximumLength(20).WithMessage(Error.MaxLength);

            RuleFor(x => x.Title).MaximumLength(200).WithMessage(Error.MaxLength);

            RuleFor(x => x.Hall).MaximumLength(50).WithMessage(Error.MaxLength);
        }
    }
}
