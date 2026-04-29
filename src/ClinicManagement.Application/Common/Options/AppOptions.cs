namespace ClinicManagement.Application.Common.Options;

/// <summary>
/// Application-level options — things the Application layer genuinely needs.
/// Infrastructure-specific config (SMTP credentials, JWT keys, etc.) lives in Infrastructure.
/// </summary>
public class AppOptions
{
    public const string Section = "App";

    /// <summary>Dashboard URL — where users land after login.</summary>
    public string FrontendUrl { get; set; } = "http://localhost:3000";

    /// <summary>Next.js auth app URL — where users are sent for login/error pages.</summary>
    public string AuthUrl { get; set; } = "http://localhost:3001";
}
