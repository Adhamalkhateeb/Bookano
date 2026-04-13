namespace Bookano.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100,ErrorMessage = "Name can't exceed 100 characters.")]
        [Required]
        public string Name { get; set; } = null!;
    }
}
