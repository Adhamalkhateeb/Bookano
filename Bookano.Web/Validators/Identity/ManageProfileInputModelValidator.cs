using Bookano.Application.Validators.Identity;
using static Bookano.Web.Areas.Identity.Pages.Account.Manage.IndexModel;

namespace Bookano.Web.Validators.Identity;

public sealed class ManageProfileInputModelValidator : ManageProfileInputValidator<InputModel>
{
    public ManageProfileInputModelValidator()
        : base(x => x.FullName, x => x.PhoneNumber)
    {
    }
}
