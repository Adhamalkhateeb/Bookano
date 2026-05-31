using Bookano.Application.Validators.Common;
using Bookano.Web.ViewModels.Publishers;

namespace Bookano.Web.Validators
{
    public class PublisherFormViewModelValidator : CommonValidator<PublisherFormViewModel>
    {
        public PublisherFormViewModelValidator() : base(x => x.Name) { }
    }
}
