using Bookano.Application.Validators;
using Bookano.Web.ViewModels.Books;

namespace Bookano.Web.Validators
{
    public class BookFormViewModelValidaor : BookCommonValidator<BookFormViewModel>
    {
        public BookFormViewModelValidaor() : base(x => x.Isbn!, x => x.Title, x => x.Hall) { }
    }
}
