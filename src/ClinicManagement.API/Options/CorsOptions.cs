namespace ClinicManagement.API.Options;

public class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = new[]
    {
        "http://localhost:5173",  // React dev server
        "http://localhost:3000",  // Next.js dev server
        "https://clinic-dashboard.vercel.app", // React production
        "https://clinic-website.vercel.app"    // Next.js production
    };
}