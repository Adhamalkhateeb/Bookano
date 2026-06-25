using Bookano.Application.DTOs.Areas;

namespace Bookano.Application.Mappings;

internal class AreaProfile : Profile
{
    public AreaProfile()
    {
        CreateMap<Area, AreaDto>()
            .ForCtorParam(
                "Governorate",
                opt => opt.MapFrom(src => src.Governorate!.Name)
            );
        CreateMap<AreaSaveDto, Area>();

    }

}
