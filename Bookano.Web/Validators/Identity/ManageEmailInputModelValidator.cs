using Bookano.Application.Validators.Identity;
using static Bookano.Web.Areas.Identity.Pages.Account.Manage.EmailModel;

namespace Bookano.Web.Validators.Identity;

public sealed class ManageEmailInputModelValidator : ManageEmailInputValidator<InputModel>
{
    public ManageEmailInputModelValidator()
        : base(x => x.NewEmail)
    {
    }
}
