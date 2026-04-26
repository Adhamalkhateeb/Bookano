namespace Bookano.Web.Core.ViewModels
{
    public class PublisherFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = Error.MaxLength), Display(Name = "Publisher")]
        [Remote("AllowItem", null!, AdditionalFields = nameof(Id), ErrorMessage = Error.Duplicated)]
        [RegularExpression(
            RegexPatterns.CharactersOnly_Eng,
            ErrorMessage = Error.OnlyEnglishLetters
        )]
        public string Name { get; set; } = null!;
    }
}
