using Bookano.Application.DTOs.Areas;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Core.Mapping
{
    public class GovernorateProfile : Profile
    {
        public GovernorateProfile()
        {
            CreateMap<GovernorateDto, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

        }
    }
}
