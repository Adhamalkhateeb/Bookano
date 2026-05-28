using Bookano.Application.Constants;

namespace Bookano.Web.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Author")]
        [Remote("AllowItem", null!, AdditionalFields = nameof(Id), ErrorMessage = Error.Duplicated)]
        public string Name { get; set; } = null!;
    }
}
