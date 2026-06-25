using Bookano.Application.DTOs.Areas;
using Bookano.Web.ViewModels.Areas;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Mapping
{
    public class AreaProfile : Profile
    {
        public AreaProfile()
        {
            CreateMap<AreaDto, AreaViewModel>();
            CreateMap<AreaFormViewModel,AreaSaveDto >().ReverseMap();
            CreateMap<AreaDto, AreaFormViewModel>().ReverseMap();


            CreateMap<AreaDto, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));
        }

    }
}
