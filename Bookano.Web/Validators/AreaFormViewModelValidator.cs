using Bookano.Application.Validators.Common;
using Bookano.Web.ViewModels.Areas;

namespace Bookano.Web.Validators;

public class AreaFormViewModelValidator : CommonValidator<AreaFormViewModel>
{
    public AreaFormViewModelValidator() : 
        base(x => x.Name) { }
}
