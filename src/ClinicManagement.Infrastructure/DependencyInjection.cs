using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Persistence;
using ClinicManagement.Infrastructure.Persistence.Seeders;
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
        services.AddHttpContextAccessor();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        services.AddScoped<IApplicationDbContext>(sp => 
            sp.GetRequiredService<ApplicationDbContext>());
        
        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            
            options.User.RequireUniqueEmail = true;
            
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<RoleSeedService>();
        services.AddScoped<SuperAdminSeedService>();
        services.AddScoped<ClinicOwnerSeedService>();
        
        services.AddScoped<DateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<CookieService>();
        services.AddScoped<IEmailTokenService, EmailTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        
        services.AddHttpClient<GeoNamesService>();
        
        services.AddHostedService<RefreshTokenCleanupService>();
        services.AddHostedService<UsageMetricsAggregationJob>();
        services.AddHostedService<EmailQueueProcessorJob>();
        services.AddHostedService<SubscriptionExpiryNotificationJob>();

        return services;
    }
}

