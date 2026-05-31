using Bookano.Application.Services.Dashboard;
using Bookano.Web.ViewModels.Dashboard;

namespace Bookano.Web.Controllers
{
    [Authorize]
    public class DashboardController(IDashboardService dashboardService, IMapper mapper) : Controller
    {
        private readonly IDashboardService _dashboardService = dashboardService;
        private readonly IMapper _mapper = mapper;

        public async Task<IActionResult> Index()
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            var viewModel = _mapper.Map<DashboardViewModel>(data);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetRentalsPerDay(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null
        )
        {
            var start = startDate is not null
                ? DateOnly.ParseExact(startDate, "yyyy-MM-dd")
                : (DateOnly?)null;

            var end = endDate is not null
                ? DateOnly.ParseExact(endDate, "yyyy-MM-dd")
                : (DateOnly?)null;

            var data = await _dashboardService.GetRentalsPerDayAsync(start, end);
            var figures = _mapper.Map<IEnumerable<ChartItemViewModel>>(data);

            return Ok(figures);
        }

        public async Task<IActionResult> GetSubscribersPerGovernorate()
        {
            var data = await _dashboardService.GetSubscribersPerGovernorateAsync();
            var result = _mapper.Map<IEnumerable<ChartItemViewModel>>(data);

            return Ok(result);
        }
    }
}
