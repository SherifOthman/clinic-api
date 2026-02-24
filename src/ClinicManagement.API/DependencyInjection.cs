using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Entities;
using ClinicManagement.API.Middleware;
using ClinicManagement.API.OpenApi;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace ClinicManagement.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment? environment = null)
    {
        services.AddHttpContextAccessor();
        //services.AddValidation();

        AddCaching(services);
        AddAuthentication(services, configuration);
        AddAuthorization(services);
        AddOptions(services, configuration);
        AddCors(services, configuration);
        AddSwagger(services);

        services.AddControllers();

        return services;
    }

    private static void AddCaching(IServiceCollection services)
    {
        services.AddMemoryCache();
    }


    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.Key)),
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    }

    private static void AddAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Requires authenticated user with clinic membership
            options.AddPolicy("RequireClinic", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("ClinicId");
            });
        });
    }


    private static void AddOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        services.Configure<GeoNamesOptions>(configuration.GetSection("GeoNames"));
        services.Configure<CorsOptions>(configuration.GetSection("Cors"));
        services.Configure<CookieSettings>(configuration.GetSection("Cookie"));
    }

    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
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
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        // Add OpenAPI with Bearer authentication transformer
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });
    }

    public static WebApplication UseAppConfigurations(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseStaticFiles();
        app.UseCors("AllowAll");
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Map OpenAPI document endpoint
        app.MapOpenApi();
        
        // Map Scalar UI
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Clinic Management API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .AddPreferredSecuritySchemes("Bearer");
        });

        return app;
    }
}
