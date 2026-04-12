namespace ClinicManagement.Infrastructure.Options;

public class JwtOptions
{
    public const string Section = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>Optional: override expiration in minutes (for testing).</summary>
    public int? RefreshTokenExpirationMinutes { get; set; }
}
