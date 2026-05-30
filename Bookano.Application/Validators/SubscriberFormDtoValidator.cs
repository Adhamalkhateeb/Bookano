using Bookano.Application.DTOs.Subscribers;
using System.Linq.Expressions;

namespace Bookano.Application.Validators;

public class SubscriberCommonValidator<T> : AbstractValidator<T> 
{
    public SubscriberCommonValidator(
        Expression<Func<T, string>> firstNameExpression,
        Expression<Func<T, string>> lastNameExpression,
        Expression<Func<T, string>> nationalIdExpression,
        Expression<Func<T, string>> emailExpression,
        Expression<Func<T, string>> mobileNumberExpression,
        Expression<Func<T, string>> addressExpression
        )
    {

        RuleFor(firstNameExpression)
                .MaximumLength(50)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.DenySpecialCharacters)
                .WithMessage(Error.DenySpecialCharacters);

        RuleFor(lastNameExpression)
            .MaximumLength(50)
            .WithMessage(Error.MaxLength)
            .Matches(RegexPatterns.DenySpecialCharacters)
            .WithMessage(Error.DenySpecialCharacters);

        RuleFor(nationalIdExpression)
            .MaximumLength(14)
            .WithMessage(Error.MaxLength)
            .Matches(RegexPatterns.NationalId)
            .WithMessage(Error.InvalidNationalId);

        RuleFor(emailExpression)
            .EmailAddress()
            .WithMessage("Invalid Email")
            .MaximumLength(150)
            .WithMessage(Error.MaxLength);

        RuleFor(mobileNumberExpression)
            .Matches(RegexPatterns.MobileNumber)
            .WithMessage(Error.InvalidMobileNumber)
            .MaximumLength(11)
            .WithMessage(Error.MaxLength);

        RuleFor(addressExpression).MaximumLength(500).WithMessage(Error.MaxLength);
    }
}

public sealed class SubscriberFormDtoValidator : SubscriberCommonValidator<SubscriberFormDto>
{
    public SubscriberFormDtoValidator() : 
        base(x => x.FirstName, x => x.LastName, x => x.NationalId, x => x.Email, x => x.MobileNumber, x => x.Address)
    {


        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage(Error.NotAllowFutureDates);

        RuleFor(x => x.AreaId).NotEmpty().WithMessage(Error.RequiredField);

        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);

        RuleFor(x => x.Image).NotNull().When(x => x.Id == 0).WithMessage(Error.EmptyImage);
    }
}
