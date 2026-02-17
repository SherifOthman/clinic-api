namespace ClinicManagement.Application.Common.Options;

public class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = new[]
    {
        "http://localhost:5173",  // React dev server
        "http://localhost:3000",  // Next.js dev server
        "http://localhost:3001",  // Dashboard dev server
        "https://clinic-dashboard-ecru.vercel.app", // Dashboard production
        "https://clinic-website-lime.vercel.app"    // Website production
    };
}
