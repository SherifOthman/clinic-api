using Hangfire.Dashboard;

namespace ClinicManagement.API.Hangfire;

/// <summary>
/// Restricts the Hangfire dashboard.
/// - Development: open to all
/// - Production: requires ?key=YOUR_SECRET query param
///   e.g. https://clinic-api.runasp.net/hangfire?key=YOUR_SECRET
/// Set HangfireDashboardKey in appsettings.Production.json to enable.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public const string DashboardKeyConfigPath = "HangfireDashboardKey";

    private readonly string? _secretKey;

    public HangfireAuthorizationFilter(string? secretKey)
    {
        _secretKey = secretKey;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        var env = httpContext.RequestServices.GetService<IWebHostEnvironment>();
        if (env?.IsDevelopment() == true) return true;

        // Production: require secret key in query string
        if (string.IsNullOrWhiteSpace(_secretKey)) return false;

        var provided = httpContext.Request.Query["key"].ToString();
        return provided == _secretKey;
    }
}
