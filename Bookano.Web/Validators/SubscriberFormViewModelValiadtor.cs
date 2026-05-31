using Bookano.Application.Validators;
using Bookano.Web.ViewModels.Subscribers;
using CloudinaryDotNet.Core;

namespace Bookano.Web.Validators
{
    public class SubscriberFormViewModelValiadtor : SubscriberCommonValidator<SubscriberFormViewModel>
    {
        public SubscriberFormViewModelValiadtor() :
            base(x => x.FirstName, x => x.LastName, x => x.NationalId, x => x.Email, x => x.MobileNumber, x => x.Address)
        { }
    }
}
