using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookano.Web.Core.ViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [
            MaxLength(100, ErrorMessage = Error.MaxLength),
            Display(Name = "Full Name"),
            RegularExpression(
                RegexPatterns.CharactersOnly_Eng,
                ErrorMessage = Error.OnlyEnglishLetters
            )
        ]
        public string FullName { get; set; } = null!;

        [
            MaxLength(50, ErrorMessage = Error.MaxLength),
            Display(Name = "User Name"),
            RegularExpression(RegexPatterns.Username, ErrorMessage = Error.InvalidUsername),
            Remote(
                "AllowUserName",
                null!,
                AdditionalFields = nameof(Id),
                ErrorMessage = Error.Duplicated
            )
        ]
        public string UserName { get; set; } = null!;

        [
            EmailAddress,
            MaxLength(200, ErrorMessage = Error.MaxLength),
            Remote(
                "AllowEmail",
                null!,
                AdditionalFields = nameof(Id),
                ErrorMessage = Error.Duplicated
            )
        ]
        public string Email { get; set; } = null!;

        [
            DataType(DataType.Password),
            StringLength(100, ErrorMessage = Error.MaxMinLength, MinimumLength = 8),
            RegularExpression(RegexPatterns.Password, ErrorMessage = Error.WeakPassword),
            RequiredIf("Id == null ", ErrorMessage = Error.RequiredField)
        ]
        public string Password { get; set; } = null!;

        [
            DataType(DataType.Password),
            Display(Name = "Confirm password"),
            Compare("Password", ErrorMessage = Error.PasswordNotMatch),
            RequiredIf("Id == null || Password != null", ErrorMessage = Error.RequiredField)
        ]
        public string ConfirmPassword { get; set; } = null!;

        public IList<string> SelectedRoles { get; set; } = new List<string>();

        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
