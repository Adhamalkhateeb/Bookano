using Bookano.Application.Validators;
using Bookano.Web.ViewModels.BookCopies;

namespace Bookano.Web.Validators
{
    public class BookCopyFormViewModelValidator : BookCopyCommonValidator<BookCopyFormViewModel>
    {
        public BookCopyFormViewModelValidator() : base(x => x.EditionNumber) { }
        
    }
}
