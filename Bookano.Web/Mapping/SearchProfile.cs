using Bookano.Application.DTOs.Books;
using Bookano.Web.ViewModels.Books;

namespace Bookano.Web.Mapping
{
    public class SearchProfile : Profile
    {
        public SearchProfile()
        {
            CreateMap<BookDetailsDto, BookViewModel>();
        }
    }
}
