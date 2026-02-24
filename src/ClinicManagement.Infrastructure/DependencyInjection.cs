using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Repositories;
using ClinicManagement.Infrastructure.Persistence.Data;
using ClinicManagement.Infrastructure.Persistence.Seeders;
using ClinicManagement.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<DbUpMigrationService>();
        services.AddScoped<SuperAdminSeedService>();
        services.AddScoped<ClinicOwnerSeedService>();
        
        services.AddScoped<DateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();
        services.AddScoped<IUserRegistrationService, UserRegistrationService>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<CookieService>();
        services.AddScoped<IEmailTokenService, EmailTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        services.AddHttpClient<GeoNamesService>();
        
        services.AddHostedService<RefreshTokenCleanupService>();
        services.AddHostedService<UsageMetricsAggregationJob>();
        services.AddHostedService<EmailQueueProcessorJob>();
        services.AddHostedService<SubscriptionExpiryNotificationJob>();

        return services;
    }
}

