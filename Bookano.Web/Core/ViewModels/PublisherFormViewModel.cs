namespace Bookano.Web.Core.ViewModels
{
    public class PublisherFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Publisher")]
        [Remote("AllowItem", null!, AdditionalFields = nameof(Id), ErrorMessage = Error.Duplicated)]
        public string Name { get; set; } = null!;
    }
}
