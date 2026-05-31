using Bookano.Application.Validators.Identity;
using static Bookano.Web.Areas.Identity.Pages.Account.Manage.ChangePasswordModel;

namespace Bookano.Web.Validators.Identity;

public sealed class ChangePasswordInputModelValidator : ChangePasswordInputValidator<InputModel>
{
    public ChangePasswordInputModelValidator()
        : base(x => x.OldPassword, x => x.NewPassword, x => x.ConfirmPassword)
    {
    }
}
