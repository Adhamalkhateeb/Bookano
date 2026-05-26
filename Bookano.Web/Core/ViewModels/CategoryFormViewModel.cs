namespace Bookano.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Category")]
        [Remote("AllowItem", null!, AdditionalFields = nameof(Id), ErrorMessage = Error.Duplicated)]
        public string Name { get; set; } = null!;
    }
}
