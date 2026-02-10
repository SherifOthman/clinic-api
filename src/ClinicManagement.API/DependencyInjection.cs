using ClinicManagement.API.Middleware;
using ClinicManagement.API.Options;
using ClinicManagement.Application.Options;
using Microsoft.OpenApi.Models;

namespace ClinicManagement.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<CorsOptions>(configuration.GetSection("Cors"));

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

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "Clinic Management API", 
                Version = "v1",
                Description = "API for managing clinic operations"
            });
        });

        return services;
    }

    public static WebApplication UseAppConfigurations(this WebApplication app)
    {
        // Domain exception middleware should come first to handle domain-specific exceptions
        app.UseMiddleware<DomainExceptionMiddleware>();
        app.UseMiddleware<GlobalExceptionMiddleware>();

        //if (app.Environment.IsDevelopment())
        //{
            app.UseSwagger();
            app.UseSwaggerUI();
        //}

        // Enable static file serving for uploaded files
        app.UseStaticFiles();

        app.UseCors("AllowAll");
        app.UseMiddleware<JwtCookieMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        
        // Map Controllers
        app.MapControllers();

        return app;
    }
}
