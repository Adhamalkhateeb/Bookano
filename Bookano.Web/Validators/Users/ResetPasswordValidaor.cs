using Bookano.Application.Validators.Users;
using Bookano.Web.ViewModels.Users;

namespace Bookano.Web.Validators.Users
{
    public class ResetPasswordValidaor : ResetPasswordCommonValidator<ResetPasswordFormViewModel>
    {
        public ResetPasswordValidaor()
            : base(x => x.Password, x => x.ConfirmPassword)
        {
        }
    }
}
