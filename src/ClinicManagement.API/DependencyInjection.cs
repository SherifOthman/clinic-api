using ClinicManagement.API.Middleware;
using ClinicManagement.API.Options; // For CorsOptions
using ClinicManagement.Application.Options; // Use Application layer JWT options
using Microsoft.OpenApi.Models;

namespace ClinicManagement.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();

        // Configure options from Application layer
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<CorsOptions>(configuration.GetSection("Cors"));

        // CORS using options
        services.AddCors(options =>
        {
            var corsOptions = configuration.GetSection("Cors").Get<CorsOptions>() ?? new CorsOptions();
            
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // Swagger - Cookie-based authentication (no JWT Bearer needed)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "Clinic Management API", 
                Version = "v1",
                Description = "API for managing clinic operations with cookie-based authentication"
            });

            // Note: No JWT Bearer authentication needed - using httpOnly cookies
            // Authentication is handled automatically by JwtCookieMiddleware
        });

        return services;
    }

    public static WebApplication UseAppConfigurations(this WebApplication app)
    {
        // Middleware pipeline
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinic Management API v1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });
        }

        app.UseCors("AllowAll");
        
        // JWT Cookie Middleware - handles token refresh for ALL protected endpoints
        app.UseMiddleware<JwtCookieMiddleware>();
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}