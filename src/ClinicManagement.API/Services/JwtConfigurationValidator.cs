using ClinicManagement.API.Options;
using ClinicManagement.Application.Options;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Services;

/// <summary>
/// Service to validate that JWT configuration is consistent between TokenService and JWT Middleware
/// </summary>
public class JwtConfigurationValidator : IValidateOptions<JwtMiddlewareOptions>
{
    private readonly IOptions<JwtOption> _jwtOptions;

    public JwtConfigurationValidator(IOptions<JwtOption> jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }

    public ValidateOptionsResult Validate(string? name, JwtMiddlewareOptions options)
    {
        var failures = new List<string>();
        var jwtOption = _jwtOptions.Value;

        // Validate that core JWT settings match between TokenService and Middleware
        if (options.Key != jwtOption.Key)
        {
            failures.Add("JWT Key mismatch between TokenService (JwtOption) and JWT Middleware (JwtMiddlewareOptions). " +
                        "Both must use the same signing key for token generation and validation to work correctly.");
        }

        if (options.Issuer != jwtOption.Issuer)
        {
            failures.Add("JWT Issuer mismatch between TokenService (JwtOption) and JWT Middleware (JwtMiddlewareOptions). " +
                        "Both must use the same issuer for token generation and validation to work correctly.");
        }

        if (options.Audience != jwtOption.Audience)
        {
            failures.Add("JWT Audience mismatch between TokenService (JwtOption) and JWT Middleware (JwtMiddlewareOptions). " +
                        "Both must use the same audience for token generation and validation to work correctly.");
        }

        // Validate key strength
        if (string.IsNullOrEmpty(options.Key))
        {
            failures.Add("JWT Key is required");
        }
        else if (options.Key.Length < 32)
        {
            failures.Add("JWT Key must be at least 32 characters long for security");
        }

        // Validate required fields
        if (string.IsNullOrEmpty(options.Issuer))
        {
            failures.Add("JWT Issuer is required");
        }

        if (string.IsNullOrEmpty(options.Audience))
        {
            failures.Add("JWT Audience is required");
        }

        // Validate time settings
        if (options.ClockSkew < TimeSpan.Zero)
        {
            failures.Add("Clock skew cannot be negative");
        }

        if (options.RefreshBufferTime < TimeSpan.Zero)
        {
            failures.Add("Refresh buffer time cannot be negative");
        }

        // Validate public paths
        if (options.PublicPaths == null || options.PublicPaths.Length == 0)
        {
            failures.Add("At least one public path must be configured");
        }

        return failures.Count > 0 
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Extension methods for JWT configuration validation
/// </summary>
public static class JwtConfigurationValidatorExtensions
{
    /// <summary>
    /// Adds JWT configuration validation to ensure TokenService and Middleware use consistent settings
    /// </summary>
    public static IServiceCollection AddJwtConfigurationValidation(this IServiceCollection services)
    {
        services.AddSingleton<IValidateOptions<JwtMiddlewareOptions>, JwtConfigurationValidator>();
        return services;
    }
}