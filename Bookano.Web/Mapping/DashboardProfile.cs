using AutoMapper;
using Bookano.Application.DTOs.Dashboard;
using Bookano.Web.ViewModels.Dashboard;

namespace Bookano.Web.Mapping;

public class DashboardProfile : Profile
{
    public DashboardProfile()
    {
        CreateMap<ChartItemDto, ChartItemViewModel>();
    }
}
