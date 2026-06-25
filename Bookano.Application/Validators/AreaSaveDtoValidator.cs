using Bookano.Application.DTOs.Areas;
using Bookano.Application.Validators.Common;

namespace Bookano.Application.Validators;


public class AreaSaveDtoValidator
    : CommonValidator<AreaSaveDto>
{
    public AreaSaveDtoValidator()
        : base(x => x.Name)
    {
    }
}



