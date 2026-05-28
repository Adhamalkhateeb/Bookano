using Bookano.Application.Validators.Common;

namespace Bookano.Web.Validators;

public class AuthorFormViewModelValidator : CommonValidator<AuthorFormViewModel>
{
    public AuthorFormViewModelValidator()
        : base(x => x.Name) { }
}