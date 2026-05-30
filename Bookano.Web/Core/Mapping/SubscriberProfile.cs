using Bookano.Application.DTOs.Subscribers;

namespace Bookano.Web.Core.Mapping
{
    public class SubscriberProfile : Profile
    {
        public SubscriberProfile()
        {
            CreateMap<SubscriberSearchResultDto, SubscriberSearchResultViewModel>();

            CreateMap<SubscriberDto, SubscriberViewModel>();

            CreateMap<SubscriberRentalDto, RentalViewModel>();

            CreateMap<SubscriberDto,SubscriberFormViewModel>().ReverseMap();

            CreateMap<SubscriberFormDto, SubscriberFormViewModel>()
             .ForMember(dest => dest.Image, opt => opt.Ignore());

            CreateMap<SubscriberFormViewModel, SubscriberFormDto>()
                .ForMember(dest => dest.Image, opt => opt.Ignore());

            CreateMap<SubscriptionDto, SubscriptionViewModel>();
        }
    }
}
