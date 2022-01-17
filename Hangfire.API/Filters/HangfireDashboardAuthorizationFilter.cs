using Hangfire.Dashboard;

namespace Hangfire.API.Filters
{
    /// <summary>
    /// Authorization filter, invoked before Hangfire Dashboard is displayed to user.
    /// </summary>
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext hangfireDashboardContext)
        {
            // TODO: implement some actual authorization to allow only Admin to access Hangfire Dashboard
            /*
            var httpContext = hangfireDashboardContext.GetHttpContext();
            return httpContext?.User?.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("admin");
            */

            return true;
        }
    }
}