using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.API.Options;

/// <summary>
/// Configuration options for JWT Cookie Middleware
/// </summary>
public class JwtMiddlewareOptions
{
    public const string SectionName = "JwtMiddleware";

    /// <summary>
    /// JWT signing key
    /// </summary>
    [Required]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// JWT issuer
    /// </summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// JWT audience
    /// </summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Clock skew tolerance for token validation
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Whether to validate the issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate the audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate the issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Public paths that should skip authentication
    /// </summary>
    public string[] PublicPaths { get; set; } = new[]
    {
        "/api/auth/login",
        "/api/auth/register",
        "/api/auth/forgot-password",
        "/api/auth/reset-password",
        "/api/auth/confirm-email",
        "/swagger",
        "/health"
    };

    /// <summary>
    /// Whether to enable automatic token refresh
    /// </summary>
    public bool EnableAutoRefresh { get; set; } = true;

    /// <summary>
    /// Whether to clear cookies on invalid tokens
    /// </summary>
    public bool ClearCookiesOnInvalidToken { get; set; } = true;

    /// <summary>
    /// Whether to enable detailed logging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Token refresh buffer time (refresh tokens this many minutes before expiry)
    /// </summary>
    public TimeSpan RefreshBufferTime { get; set; } = TimeSpan.FromMinutes(5);
}