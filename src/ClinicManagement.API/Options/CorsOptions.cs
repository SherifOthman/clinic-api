namespace ClinicManagement.API.Options;

public class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = new[]
    {
        "http://localhost:5173",  // React dev server
        "http://localhost:3000",  // Next.js dev server
        "http://localhost:3001",  // Dashboard dev server
        "http://192.168.1.5:3000", // Local network access
        "https://clinic-dashboard-ecru.vercel.app", // Dashboard production
        "https://clinic-website-lime.vercel.app"    // Website production
    };
}
