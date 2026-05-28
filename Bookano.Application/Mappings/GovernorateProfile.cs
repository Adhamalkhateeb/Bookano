using Bookano.Application.DTOs.Governorates;

namespace Bookano.Application.Mappings;

public class GovernorateProfile  : Profile
{
    public GovernorateProfile()
    {
        CreateMap<Governorate, GovernorateDto>();
    }
}
