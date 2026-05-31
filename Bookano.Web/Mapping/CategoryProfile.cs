using Bookano.Application.DTOs.Categories;
using Bookano.Web.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Bookano.Web.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryDto, CategoryViewModel>();
        CreateMap<CategoryDto, CategoryFormViewModel>().ReverseMap();
        CreateMap<CategoryFormViewModel, CategoryFormDto>().ReverseMap();


        CreateMap<CategoryDto, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

    }
}