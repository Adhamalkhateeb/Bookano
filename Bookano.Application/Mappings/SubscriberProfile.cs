using Bookano.Application.DTOs.Subscribers;

namespace Bookano.Application.Mappings;

public class SubscriberProfile : Profile
{
    public SubscriberProfile()
    {



        CreateMap<Subscriber, SubscriberSaveDto>()
            .ForMember(dest => dest.GovernorateId, opt => opt.MapFrom(src => src.Area!.GovernorateId));

        CreateMap<SubscriberSaveDto, Subscriber>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ImageThumbnailUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ImagePublicId, opt => opt.Ignore())
            .ForMember(dest => dest.Area, opt => opt.Ignore())
            .ForMember(dest => dest.Subscriptions, opt => opt.Ignore())
            .ForMember(dest => dest.Rentals, opt => opt.Ignore());

        CreateMap<Subscription, SubscriptionDto>();

        CreateMap<Subscriber, SubscriberDto>()
            .ForMember(dest => dest.GovernorateId, opt => opt.MapFrom(src => src.Area!.GovernorateId))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area!.Name))
            .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Area!.Governorate!.Name));
    }
}
