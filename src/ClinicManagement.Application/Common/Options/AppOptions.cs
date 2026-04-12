namespace ClinicManagement.Application.Common.Options;

/// <summary>
/// Application-level options — things the Application layer genuinely needs.
/// Infrastructure-specific config (SMTP credentials, JWT keys, etc.) lives in Infrastructure.
/// </summary>
public class AppOptions
{
    public const string Section = "App";

    /// <summary>Frontend base URL — used to build links in emails (password reset, invitations).</summary>
    public string FrontendUrl { get; set; } = "http://localhost:3000";
}
