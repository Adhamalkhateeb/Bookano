using Bookano.Application.DTOs.Categories;
using Bookano.Application.Validators.Common;

namespace Bookano.Application.Validators;

public class CategorySaveDtoValidator : CommonValidator<CategorySaveDto>
{
    public CategorySaveDtoValidator()
        : base(x => x.Name)
    {}
}




