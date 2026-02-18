using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Repositories;
using ClinicManagement.Infrastructure.Data;
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
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<DbUpMigrationService>();

        services.AddScoped<PhoneValidationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<CookieService>();
        services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<MailKitSmtpClient>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<SuperAdminSeedService>();
        
        services.AddHttpClient<GeoNamesService>();
        
        services.AddHostedService<RefreshTokenCleanupService>();

        return services;
    }
}

