using CloudinaryDotNet.Core;

namespace Bookano.Web.Validators
{
    public class SubscriberValiadtor : AbstractValidator<SubscriberFormViewModel>
    {
        public SubscriberValiadtor()
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(50)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.DenySpecialCharacters)
                .WithMessage(Error.DenySpecialCharacters);

            RuleFor(x => x.LastName)
                .MaximumLength(50)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.DenySpecialCharacters)
                .WithMessage(Error.DenySpecialCharacters);

            RuleFor(x => x.NationalId)
                .MaximumLength(14)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.NationalId)
                .WithMessage(Error.InvalidNationalId);

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Invalid Email")
                .MaximumLength(150)
                .WithMessage(Error.MaxLength);

            RuleFor(x => x.MobileNumber)
                .Matches(RegexPatterns.MobileNumber)
                .WithMessage(Error.InvalidMobileNumber)
                .MaximumLength(11)
                .WithMessage(Error.MaxLength);

            RuleFor(x => x.Address).MaximumLength(500).WithMessage(Error.MaxLength);
        }
    }
}
