using Bookano.Application.Validators.Common;
using Bookano.Web.ViewModels.Authors;

namespace Bookano.Web.Validators;

public class AuthorFormViewModelValidator : CommonValidator<AuthorFormViewModel>
{
    public AuthorFormViewModelValidator()
        : base(x => x.Name) { }
}