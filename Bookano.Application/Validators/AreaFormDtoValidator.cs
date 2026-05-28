using Bookano.Application.DTOs.Areas;
using Bookano.Application.Validators.Common;

namespace Bookano.Application.Validators;


public class AreaFormDtoValidator
    : CommonValidator<AreaFormDto>
{
    public AreaFormDtoValidator()
        : base(x => x.Name)
    {
    }
}



