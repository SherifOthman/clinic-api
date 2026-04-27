namespace ClinicManagement.Infrastructure.Options;

public class GoogleOAuthOptions
{
    public const string Section = "Google";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
