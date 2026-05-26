using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookano.Web.Core.ViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Display(Name = "User Name")]
        [Remote(
            "AllowUserName",
            null!,
            AdditionalFields = nameof(Id),
            ErrorMessage = Error.Duplicated
        )]
        public string UserName { get; set; } = null!;

        [Remote(
            "AllowEmail",
            null!,
            AdditionalFields = nameof(Id),
            ErrorMessage = Error.Duplicated
        )]
        public string Email { get; set; } = null!;

        [DataType(DataType.Password)]
        [RequiredIf("Id == null ", ErrorMessage = Error.RequiredField)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [RequiredIf("Id == null || Password != null", ErrorMessage = Error.RequiredField)]
        public string? ConfirmPassword { get; set; } = null!;

        public IList<string> SelectedRoles { get; set; } = [];

        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
