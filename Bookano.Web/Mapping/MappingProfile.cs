using Bookano.Application.DTOs.Rentals;
using Bookano.Application.DTOs.Subscribers;

namespace Bookano.Web.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DateOnly, DateTime>().ConvertUsing(d => d.ToDateTime(TimeOnly.MinValue));

        CreateMap<DateTime, DateOnly>().ConvertUsing(d => DateOnly.FromDateTime(d));
    }
}
