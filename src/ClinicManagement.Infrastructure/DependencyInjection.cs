using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        // Register IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Identity
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Services
        services.AddScoped<DateTimeProvider>();
        services.AddScoped<CurrentUserService>();
        services.AddScoped<CodeGeneratorService>();
        services.AddScoped<PhoneValidationService>();
        services.AddScoped<UserRegistrationService>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<TokenService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<CookieService>();
        services.AddScoped<EmailConfirmationService>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<MailKitSmtpClient>();
        services.AddScoped<LocalFileStorageService>();
        services.AddScoped<DatabaseInitializationService>();
        services.AddScoped<ComprehensiveSeedService>();
        
        // External API Services
        services.AddHttpClient<GeoNamesService>();
        
        // Background Services
        services.AddHostedService<RefreshTokenCleanupService>();

        return services;
    }
}
