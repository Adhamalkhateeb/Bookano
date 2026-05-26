namespace Bookano.Web.Validators
{
    public class RentalReturnValidator : AbstractValidator<RentalReturnFormViewModel>
    {
        public RentalReturnValidator()
        {
            RuleFor(x => x.PenalityPaid)
                .Must((model, paid) => model.TotalDelayInDays == 0 || paid)
                .WithMessage(Error.PenalityShouldBePaid);
        }
    }
}
