using Bookano.Application.DTOs.Areas;

namespace Bookano.Application.Mappings;

internal class AreaProfile : Profile
{
    public AreaProfile()
    {
        CreateMap<Area, AreaDto>()
            .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate!.Name));
        CreateMap<AreaFormDto, Area>();

        CreateMap<Governorate, GovernorateDto>();
    }
}
