using Bookano.Application.DTOs.Rentals;

namespace Bookano.Application.Validators.Rentals;

public class RentalFormDtoValidator : AbstractValidator<RentalFormDto>
{
    public RentalFormDtoValidator()
    {
        RuleFor(x => x.SubscriberId).GreaterThan(0);

        RuleFor(x => x.SelectedCopies).NotEmpty().WithMessage(Error.RequiredField);
    }
}
