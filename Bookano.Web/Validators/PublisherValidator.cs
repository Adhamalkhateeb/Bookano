namespace Bookano.Web.Validators
{
    public class PublisherValidator : AbstractValidator<PublisherFormViewModel>
    {
        public PublisherValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.CharactersOnly_Eng)
                .WithMessage(Error.OnlyEnglishLetters);
        }
    }
}
