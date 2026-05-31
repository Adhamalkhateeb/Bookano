using Bookano.Application.DTOs.Subscribers;
using Bookano.Web.ViewModels.Rentals;
using Bookano.Web.ViewModels.Subscribers;

namespace Bookano.Web.Mapping
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
