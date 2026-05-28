using Bookano.Application.Validators.Common;

namespace Bookano.Web.Validators;

public class AreaFormViewModelValidator : CommonValidator<AreaFormViewModel>
{
    public AreaFormViewModelValidator() : 
        base(x => x.Name) { }
}
