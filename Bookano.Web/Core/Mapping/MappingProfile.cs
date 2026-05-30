using Bookano.Application.DTOs.Subscribers;

namespace Bookano.Web.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DateOnly, DateTime>().ConvertUsing(d => d.ToDateTime(TimeOnly.MinValue));

        CreateMap<DateTime, DateOnly>().ConvertUsing(d => DateOnly.FromDateTime(d));

        //Rentals
        CreateMap<Rental, RentalViewModel>();
        CreateMap<RentalCopy, RentalCopyViewModel>();


        //Users
        CreateMap<ApplicationUser, UserViewModel>()
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(u => u.CreatedOnUtc))
            .ForMember(dest => dest.LastUpdatedOn, opt => opt.MapFrom(u => u.LastUpdatedOnUtc));

        CreateMap<UserFormViewModel, ApplicationUser>()
            .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(u => u.Email.ToUpper()))
            .ForMember(
                dest => dest.NormalizedUserName,
                opt => opt.MapFrom(u => u.UserName.ToUpper())
            )
            .ReverseMap();
    }
}
