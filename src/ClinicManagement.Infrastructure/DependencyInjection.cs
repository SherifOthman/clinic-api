using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Infrastructure.Services;
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
        services.AddScoped<ISecurityAuditWriter, SecurityAuditWriter>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<CookieService>();
        services.AddScoped<IEmailTokenService, EmailTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.AddHttpClient<GeoNamesService>();

        services.AddHostedService<RefreshTokenCleanupService>();
        services.AddHostedService<AuditLogCleanupService>();
        services.AddHostedService<UsageMetricsAggregationJob>();
        services.AddHostedService<EmailQueueProcessorJob>();
        services.AddHostedService<SubscriptionExpiryNotificationJob>();

        return services;
    }
}
