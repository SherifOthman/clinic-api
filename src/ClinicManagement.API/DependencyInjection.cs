using ClinicManagement.API.Middleware;
using ClinicManagement.API.OpenApi;
using Scalar.AspNetCore;

namespace ClinicManagement.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();

        // Add OpenAPI with Bearer authentication transformer
        services.AddEndpointsApiExplorer();
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        // Add CORS
        var allowedOrigins = new[] { "http://localhost:5173", "http://localhost:3000" }; 
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    public static WebApplication UseAppConfigurations(this WebApplication app)
    {
        // Global exception middleware should be first
        app.UseMiddleware<GlobalExceptionMiddleware>();
        
        // Map OpenAPI endpoint
        app.MapOpenApi();
        
        // Configure Scalar UI
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Clinic Management API")
                .WithTheme(ScalarTheme.Default)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();

        return app;
    }
}
