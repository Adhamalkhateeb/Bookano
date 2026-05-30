using Bookano.Application.DTOs.Subscribers;

namespace Bookano.Application.Mappings;

public class SubscriberProfile : Profile
{
    public SubscriberProfile()
    {

        CreateMap<Subscriber, SubscriberSearchResultDto>()
           .ForMember(
               dest => dest.FullName,
               opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}")
           );       

        CreateMap<Subscriber, SubscriberFormDto>()
            .ForMember(dest => dest.GovernorateId, opt => opt.MapFrom(src => src.Area!.GovernorateId));

        CreateMap<SubscriberFormDto, Subscriber>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ImageThumbnailUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ImagePublicId, opt => opt.Ignore())
            .ForMember(dest => dest.Area, opt => opt.Ignore())
            .ForMember(dest => dest.Subscriptions, opt => opt.Ignore())
            .ForMember(dest => dest.Rentals, opt => opt.Ignore());

        CreateMap<Subscription, SubscriptionDto>();
    }
}
