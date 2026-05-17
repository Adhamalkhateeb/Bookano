using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Bookano.Web.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _policyName;

        public HangfireAuthorizationFilter(string policyName)
        {
            _policyName = policyName;
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            var authorizationService =
                httpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            var result = authorizationService
                .AuthorizeAsync(httpContext.User, null, _policyName)
                .GetAwaiter()
                .GetResult();

            return result.Succeeded;
        }
    }
}
