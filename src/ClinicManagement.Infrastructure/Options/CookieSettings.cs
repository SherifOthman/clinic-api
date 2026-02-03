namespace ClinicManagement.Infrastructure.Options;

public class CookieSettings
{
    public int ExpiryInMinutes { get; set; } = 15;
    public int RefreshTokenExpiryInDays { get; set; } = 7;
    public bool IsProduction { get; set; } = false;
    public string? CookieDomain { get; set; }
}
