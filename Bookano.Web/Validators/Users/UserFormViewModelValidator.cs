using Bookano.Application.Validators.Users;
using Bookano.Web.ViewModels.Users;

namespace Bookano.Web.Validators.Users
{
    public class UserFormViewModelValidator : UserCommonValidator<UserFormViewModel>
    {
        public UserFormViewModelValidator()
            : base(x => x.FullName, x => x.UserName, x => x.Email)
        {
            RuleFor(x => x.Password)
                .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Id))
                .WithMessage(Error.RequiredField)
                .Length(8, 100)
                .WithMessage(Error.MaxMinLength)
                .Matches(RegexPatterns.Password)
                .WithMessage(Error.WeakPassword)
                .When(x => !string.IsNullOrEmpty(x.Password));

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Id))
                .WithMessage(Error.RequiredField)
                .Equal(x => x.Password)
                .WithMessage(Error.PasswordNotMatch);
        }
    }
}
