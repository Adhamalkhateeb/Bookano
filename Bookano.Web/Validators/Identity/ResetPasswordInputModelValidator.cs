using Bookano.Application.Validators.Identity;
using static Bookano.Web.Areas.Identity.Pages.Account.ResetPasswordModel;

namespace Bookano.Web.Validators.Identity;

public sealed class ResetPasswordInputModelValidator : ResetPasswordInputValidator<InputModel>
{
    public ResetPasswordInputModelValidator()
        : base(x => x.Email, x => x.Password, x => x.ConfirmPassword)
    {
    }
}
