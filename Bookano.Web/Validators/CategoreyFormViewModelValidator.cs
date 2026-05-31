using Bookano.Application.Validators.Common;
using Bookano.Web.ViewModels.Categories;

namespace Bookano.Web.Validators
{
    public class CategoreyFormViewModelValidator : CommonValidator<CategoryFormViewModel>
    {
        public CategoreyFormViewModelValidator() : base(x => x.Name)
        { }
    }
}
