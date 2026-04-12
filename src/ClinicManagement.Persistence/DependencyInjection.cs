using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Unit of Work (contains all repositories)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireDigit           = true;
            options.Password.RequireLowercase       = true;
            options.Password.RequireUppercase       = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength         = 8;

            options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers      = true;

            options.User.RequireUniqueEmail         = true;
            options.SignIn.RequireConfirmedEmail     = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.Section));
        services.AddScoped<RoleSeedService>();
        services.AddScoped<SpecializationSeedService>();
        services.AddScoped<ChronicDiseaseSeedService>();
        services.AddScoped<SubscriptionPlanSeedService>();
        services.AddScoped<DemoUsersSeedService>();

        return services;
    }
}
