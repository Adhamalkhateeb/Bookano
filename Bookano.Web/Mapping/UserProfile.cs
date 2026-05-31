using Bookano.Application.DTOs.Users;
using Bookano.Web.ViewModels.Users;

namespace Bookano.Web.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto, UserViewModel>();

            CreateMap<UserFormViewModel, UserFormDto>().ReverseMap();

            CreateMap<ResetPasswordFormViewModel, UserResetPasswordDto>();
        }
    }
}
