namespace ClinicManagement.Application.Common.Options;

/// <summary>
/// Application-level options — things the Application layer genuinely needs.
/// Infrastructure-specific config (SMTP credentials, JWT keys, etc.) lives in Infrastructure.
/// </summary>
public class AppOptions
{
    public const string Section = "App";

    /// <summary>
    /// Dashboard app URL (clinic-dashboard).
    /// Used in email links: email confirmation, password reset, staff invitations.
    /// Also used as the OAuth redirect destination after a successful Google login.
    /// </summary>
    public string DashboardUrl { get; set; } = "http://localhost:3000";

    /// <summary>
    /// Website app URL (clinic-website).
    /// Used only for auth redirects: login page and OAuth error pages.
    /// </summary>
    public string WebsiteUrl { get; set; } = "http://localhost:3001";
}
