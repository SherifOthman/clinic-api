namespace ClinicManagement.Application.Options;

/// <summary>
/// JWT configuration options for the application.
/// Contains all JWT-related settings including token expiration, signing keys, and public paths.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Secret key used for signing JWT tokens
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// JWT token issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// JWT token audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Access token expiration time in minutes (default: 15 minutes)
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    
    /// <summary>
    /// Refresh token expiration time in days (default: 7 days)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
    
    /// <summary>
    /// Public paths that don't require authentication
    /// </summary>
    public string[] PublicPaths { get; set; } = new[]
    {
        "/api/auth/register",
        "/api/auth/login",
        "/api/auth/forgot-password",
        "/api/auth/reset-password",
        "/api/auth/confirm-email",
        "/api/auth/resend-email-verification",
        "/api/staff/accept-invitation",
        "/api/subscriptionplans",
        "/swagger",
        "/health"
    };
}