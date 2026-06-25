using Bookano.Application.DTOs.Authors;
using Bookano.Application.Validators.Common;

namespace Bookano.Application.Validators;


public sealed class AuthorSaveDtoValidator : CommonValidator<AuthorSaveDto>
{
    public AuthorSaveDtoValidator()
        : base(x => x.Name)
    {
    }
}



