namespace Bookano.Web.Validators
{
    public class AreaValidator : AbstractValidator<AreaFormViewModel>
    {
        public AreaValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.AreaName)
                .WithMessage(Error.InvalidAreaName);
        }
    }
}
