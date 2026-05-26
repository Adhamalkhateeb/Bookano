namespace Bookano.Web.Validators
{
    public class AuthorValidator : AbstractValidator<AuthorFormViewModel>
    {
        public AuthorValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage(Error.MaxLength)
                .Matches(RegexPatterns.CharactersOnly_Eng)
                .WithMessage(Error.OnlyEnglishLetters);
        }
    }
}
