using Bookano.Web.Core.Consts;

namespace Bookano.Web.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = Error.MaxLength),Display(Name="Author")]
        [Remote("AllowItem", null!, AdditionalFields = nameof(Id),ErrorMessage = Error.Duplicated)]
        public string Name { get; set; } = null!;
    }
}
