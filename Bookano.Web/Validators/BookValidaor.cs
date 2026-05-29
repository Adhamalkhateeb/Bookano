using Bookano.Application.Validators;

namespace Bookano.Web.Validators
{
    public class BookValidaor : BookCommonValidator<BookFormViewModel>
    {
        public BookValidaor() : base(x => x.Isbn!, x => x.Title, x => x.Hall) { }
    }
}
