using ClinicManagement.API.Middleware;
using ClinicManagement.API.Options;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Extensions;

/// <summary>
/// Extension methods for configuring JWT Cookie Middleware
/// </summary>
public static class JwtMiddlewareExtensions
{
    /// <summary>
    /// Adds JWT Cookie Middleware with options pattern configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddJwtCookieMiddleware(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configure options with validation
        services.Configure<JwtMiddlewareOptions>(configuration.GetSection(JwtMiddlewareOptions.SectionName));
        
        // Add options validation
        services.AddSingleton<IValidateOptions<JwtMiddlewareOptions>, JwtMiddlewareOptionsValidator>();
        
        return services;
    }

    /// <summary>
    /// Adds JWT Cookie Middleware with custom configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddJwtCookieMiddleware(
        this IServiceCollection services, 
        Action<JwtMiddlewareOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        // Add options validation
        services.AddSingleton<IValidateOptions<JwtMiddlewareOptions>, JwtMiddlewareOptionsValidator>();
        
        return services;
    }

    /// <summary>
    /// Uses JWT Cookie Middleware in the request pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseJwtCookieMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<JwtCookieMiddleware>();
    }
}

/// <summary>
/// Validator for JWT Middleware Options
/// </summary>
public class JwtMiddlewareOptionsValidator : IValidateOptions<JwtMiddlewareOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtMiddlewareOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrEmpty(options.Key))
        {
            failures.Add("JWT Key is required");
        }
        else if (options.Key.Length < 32)
        {
            failures.Add("JWT Key must be at least 32 characters long for security");
        }

        if (string.IsNullOrEmpty(options.Issuer))
        {
            failures.Add("JWT Issuer is required");
        }

        if (string.IsNullOrEmpty(options.Audience))
        {
            failures.Add("JWT Audience is required");
        }

        if (options.ClockSkew < TimeSpan.Zero)
        {
            failures.Add("Clock skew cannot be negative");
        }

        if (options.RefreshBufferTime < TimeSpan.Zero)
        {
            failures.Add("Refresh buffer time cannot be negative");
        }

        if (options.PublicPaths == null || options.PublicPaths.Length == 0)
        {
            failures.Add("At least one public path must be configured");
        }

        return failures.Count > 0 
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}