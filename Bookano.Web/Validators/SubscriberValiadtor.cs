using Bookano.Application.Validators;
using CloudinaryDotNet.Core;

namespace Bookano.Web.Validators
{
    public class SubscriberValiadtor : SubscriberCommonValidator<SubscriberFormViewModel>
    {
        public SubscriberValiadtor() :
            base(x => x.FirstName, x => x.LastName, x => x.NationalId, x => x.Email, x => x.MobileNumber, x => x.Address)
        { }
    }
}
