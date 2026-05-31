using Bookano.Application.DTOs.Dashboard;
using Bookano.Web.ViewModels.Books;
using Bookano.Web.ViewModels.Dashboard;

namespace Bookano.Web.Mapping
{
    public class DashboardProfile : Profile
    {
        public DashboardProfile()
        {
            CreateMap<DashboardBookDto, BookViewModel>();
            CreateMap<DashboardDto, DashboardViewModel>();
            CreateMap<ChartItemDto, ChartItemViewModel>();
        }
    }
}
