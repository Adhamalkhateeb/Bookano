using Bookano.Application.Validators;

namespace Bookano.Web.Validators
{
    public class BookCopyValidator : BookCopyCommonValidator<BookCopyFormViewModel>
    {
        public BookCopyValidator() : base(x => x.EditionNumber) { }
        
    }
}
