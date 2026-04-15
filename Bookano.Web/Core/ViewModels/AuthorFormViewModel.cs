namespace Bookano.Web.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "Name can't exceed 100 characters.")]
        [Remote("AllowItem", null, ErrorMessage = "Author already exists.", AdditionalFields = nameof(Id))]
        public string Name { get; set; } = null!;
    }
}
