using Bookano.Application.Validators.Common;

namespace Bookano.Web.Validators
{
    public class CategoreyFormViewModelValidator : CommonValidator<CategoryFormViewModel>
    {
        public CategoreyFormViewModelValidator() : base(x => x.Name)
        { }
    }
}
