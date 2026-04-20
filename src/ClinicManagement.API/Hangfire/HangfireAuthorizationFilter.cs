using Hangfire.Dashboard;

namespace ClinicManagement.API.Hangfire;

/// <summary>
/// Restricts the Hangfire dashboard to SuperAdmin users only.
/// In development, allows all requests for convenience.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow in development
        var env = httpContext.RequestServices.GetService<IWebHostEnvironment>();
        if (env?.IsDevelopment() == true) return true;

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("SuperAdmin");
    }
}
