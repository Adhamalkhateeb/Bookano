using Bookano.Application.Validators.Identity;
using static Bookano.Web.Areas.Identity.Pages.Account.ForgotPasswordModel;

namespace Bookano.Web.Validators.Identity;

public sealed class ForgotPasswordInputModelValidator : ForgotPasswordInputValidator<InputModel>
{
    public ForgotPasswordInputModelValidator()
        : base(x => x.Email)
    {
    }
}
