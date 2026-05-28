using Bookano.Application.DTOs.Authors;
using Bookano.Application.Validators.Common;

namespace Bookano.Application.Validators;


public class AuthorFormDtoValidator
    : CommonValidator<AuthorFormDto>
{
    public AuthorFormDtoValidator()
        : base(x => x.Name)
    {
    }
}



