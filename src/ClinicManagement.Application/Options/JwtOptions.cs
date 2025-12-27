namespace ClinicManagement.Application.Options;

/// <summary>
/// JWT configuration options for the application.
/// Contains all JWT-related settings including token expiration and signing keys.
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
}