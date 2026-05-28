using Bookano.Application.DTOs.Categories;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Bookano.Web.Core.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryDto, CategoryViewModel>();
        CreateMap<CategoryDto, CategoryFormViewModel>().ReverseMap();
        CreateMap<CategoryFormViewModel, CategoryFormDto>().ReverseMap();


        //TODO: Replace Model with dto
        CreateMap<Category, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

    }
}