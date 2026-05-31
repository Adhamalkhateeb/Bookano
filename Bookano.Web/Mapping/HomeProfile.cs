using Bookano.Application.DTOs.Home;
using Bookano.Web.ViewModels.Books;

namespace Bookano.Web.Mapping
{
    public class HomeProfile : Profile
    {
        public HomeProfile()
        {
            CreateMap<HomeBookDto, BookViewModel>();
        }
    }
}
