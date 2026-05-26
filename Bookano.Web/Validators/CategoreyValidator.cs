namespace Bookano.Web.Validators
{
    public class CategoreyValidator : AbstractValidator<CategoryFormViewModel>
    {
        public CategoreyValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.CharactersOnly_Eng)
                .WithMessage(Error.OnlyEnglishLetters);
        }
    }
}
