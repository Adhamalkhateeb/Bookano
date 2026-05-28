using Bookano.Application.DTOs.Categories;
using Bookano.Application.Validators.Common;

namespace Bookano.Application.Validators;

public class CategoryFormDtoValidator : CommonValidator<CategoryFormDto>
{
    public CategoryFormDtoValidator()
        : base(x => x.Name)
    {}
}




