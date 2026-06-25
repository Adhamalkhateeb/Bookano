using Bookano.Application.DTOs.Rentals;

namespace Bookano.Application.Mappings;

public class RentalProfile : Profile
{
    public RentalProfile()
    {
        CreateMap<RentalCopy, RentalCopyDto>()
            .ForMember(dest => dest.BookCopy, opt => opt.MapFrom(src => src.BookCopy))
            .ForMember(dest => dest.Subscriber, opt => opt.MapFrom(src => src.Rental!.Subscriber))
            .ReverseMap();

        CreateMap<Rental, RentalDto>();
    }
}
