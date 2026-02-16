namespace ClinicManagement.API.Common.Options;

/// <summary>
/// Cookie configuration settings for refresh tokens
/// </summary>
public class CookieSettings
{
    /// <summary>
    /// Refresh token cookie expiration in days
    /// </summary>
    public int RefreshTokenExpiryInDays { get; set; } = 30;
    
    /// <summary>
    /// Whether running in production (enables Secure flag for HTTPS)
    /// </summary>
    public bool IsProduction { get; set; } = false;
    
    /// <summary>
    /// Optional cookie domain (null for current domain)
    /// </summary>
    public string? CookieDomain { get; set; }
}
