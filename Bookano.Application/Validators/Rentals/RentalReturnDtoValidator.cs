using Bookano.Application.DTOs.Rentals;

namespace Bookano.Application.Validators.Rentals;

public class RentalReturnDtoValidator : AbstractValidator<RentalReturnDto>
{
    public RentalReturnDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.RentalCopies).NotEmpty().WithMessage(Error.RequiredField);

        RuleFor(x => x.PenalityPaid)
            .Must((model, paid) => model.TotalDelayInDays == 0 || paid)
            .WithMessage(Error.PenalityShouldBePaid);
    }
}
