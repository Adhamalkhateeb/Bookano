using Bookano.Application.Validators.Common;

namespace Bookano.Web.Validators
{
    public class PublisherFormViewModelValidator : CommonValidator<PublisherFormViewModel>
    {
        public PublisherFormViewModelValidator() : base(x => x.Name) { }
    }
}
