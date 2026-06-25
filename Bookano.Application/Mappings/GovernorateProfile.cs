using Bookano.Application.DTOs.Governorates;

namespace Bookano.Application.Mappings;

internal class GovernorateProfile : Profile
{
    public GovernorateProfile()
    {
        CreateMap<Governorate, GovernorateDto>();
    }

}