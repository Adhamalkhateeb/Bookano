using Bookano.Application.DTOs.Rentals;

namespace Bookano.Application.Validators.Rentals;

public class RentalSaveDtoValidator : AbstractValidator<RentalSaveDto>
{
    public RentalSaveDtoValidator()
    {
        RuleFor(x => x.SubscriberId).GreaterThan(0);

        RuleFor(x => x.SelectedCopies).NotEmpty().WithMessage(Error.RequiredField);
    }
}
