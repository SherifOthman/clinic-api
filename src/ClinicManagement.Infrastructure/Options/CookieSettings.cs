namespace ClinicManagement.Infrastructure.Options;

public class CookieSettings
{
    public const string Section = "Cookie";

    public int RefreshTokenExpiryInDays { get; set; } = 30;
    public bool IsProduction { get; set; } = false;
    public string? CookieDomain { get; set; }
}
