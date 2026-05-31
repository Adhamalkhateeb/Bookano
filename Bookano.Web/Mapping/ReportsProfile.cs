using Bookano.Application.DTOs.Reports;
using Bookano.Web.ViewModels.Books;
using Bookano.Web.ViewModels.Reports;

namespace Bookano.Web.Mapping
{
    public class ReportsProfile : Profile
    {
        public ReportsProfile()
        {
            CreateMap<BookReportDto, BookViewModel>();
            CreateMap<RentalsReportDto, RentalsReportItemViewModel>();
            CreateMap<DelayedRentalDto, DelayedRentalItemViewModel>();
        }
    }
}
