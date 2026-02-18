using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.API.Middleware;
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
        services.AddValidation();

        AddCaching(services);
        AddDatabase(services, configuration, environment);
        AddIdentity(services);
        AddAuthentication(services, configuration);
        AddAuthorization(services);
        AddApplicationServices(services);
        AddOptions(services, configuration);
        AddCors(services, configuration);
        AddSwagger(services);

        services.AddControllers();

        return services;
    }

    private static void AddCaching(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder => builder.NoCache());
            options.AddPolicy("ReferenceData", builder => builder.Expire(TimeSpan.FromHours(1)));
            options.AddPolicy("LocationData", builder => builder.Expire(TimeSpan.FromHours(24)));
        });
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment? environment)
    {
    }

    private static void AddIdentity(IServiceCollection services)
    {
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
            options.AddPolicy("SuperAdminOnly", policy =>
                policy.RequireRole(Roles.SuperAdmin));

            options.AddPolicy("ClinicManagement", policy =>
                policy.RequireRole(Roles.ClinicOwner));

            options.AddPolicy("MedicalStaff", policy =>
                policy.RequireRole(Roles.Doctor, Roles.ClinicOwner));

            options.AddPolicy("StaffAccess", policy =>
                policy.RequireRole(Roles.Doctor, Roles.Receptionist, Roles.ClinicOwner));

            options.AddPolicy("InventoryManagement", policy =>
                policy.RequireRole(Roles.ClinicOwner, Roles.Doctor));
        });
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
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
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        });
    }

    public static WebApplication UseAppConfigurations(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseStaticFiles();
        app.UseCors("AllowAll");
        app.UseOutputCache();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });

        app.MapScalarApiReference();

        return app;
    }
}
