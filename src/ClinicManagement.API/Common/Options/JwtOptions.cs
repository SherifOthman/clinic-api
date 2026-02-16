namespace ClinicManagement.API.Common.Options;

public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    
    /// <summary>
    /// Optional: Override refresh token expiration in minutes (for testing).
    /// If set, this takes precedence over RefreshTokenExpirationDays.
    /// </summary>
    public int? RefreshTokenExpirationMinutes { get; set; }
}
