namespace Bookano.Web.ViewModels.Categories
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Category")]
        [Remote("AllowItem", null!, AdditionalFields = nameof(Id), ErrorMessage = Error.Duplicated)]
        public string Name { get; set; } = null!;
    }
}
