namespace ClinicManagement.Infrastructure.Options;

public class CorsOptions
{
    public const string Section = "Cors";

    public string[] AllowedOrigins { get; set; } =
    [
        "http://localhost:5173",
        "http://localhost:3000",
        "http://localhost:3001",
        "https://clinic-dashboard-ecru.vercel.app",
        "https://clinic-website-lime.vercel.app",
    ];
}
