using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<DateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ISecurityAuditWriter, SecurityAuditWriter>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<CookieService>();
        services.AddScoped<IEmailTokenService, EmailTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IPhoneNormalizer, PhoneNormalizer>();

        services.AddHttpClient<GeoNamesService>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
        });
        services.AddScoped<IGeoNamesService>(sp => sp.GetRequiredService<GeoNamesService>());

        // Hangfire jobs (scoped — resolved per execution by Hangfire's DI scope)
        services.AddScoped<EmailQueueProcessorJob>();
        services.AddScoped<AuditLogCleanupService>();
        services.AddScoped<RefreshTokenCleanupService>();
        services.AddScoped<UsageMetricsAggregationJob>();
        services.AddScoped<SubscriptionExpiryNotificationJob>();

        // Hangfire storage config only — server + dashboard registered in API layer
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing");

        GlobalConfiguration.Configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout   = TimeSpan.FromMinutes(5),
                QueuePollInterval            = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks           = true,
            });

        return services;
    }
}
