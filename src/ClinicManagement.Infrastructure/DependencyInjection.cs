using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ClinicManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Memory Cache (required for caching service and rate limiting)
        services.AddMemoryCache();

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        
        // Register IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Identity
        services.AddIdentity<User, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication - Minimal configuration since middleware handles token validation
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOption>() 
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
            
            // Minimal token validation - middleware handles the actual validation
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.Key)),
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = false, // Middleware handles lifetime validation
                ClockSkew = TimeSpan.Zero
            };

            // Disable JWT Bearer token extraction - middleware handles everything
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Don't extract tokens here - let middleware handle it
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    // Suppress default challenge response - middleware handles auth
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        });

        // Authorization Policies
        services.AddAuthorizationBuilder()
            // Role-based policies
            .AddPolicy(Application.Common.Authorization.Policies.RequireClinicOwner, policy =>
                policy.RequireRole(Domain.Common.Enums.UserRole.ClinicOwner.ToString()))
            .AddPolicy(Application.Common.Authorization.Policies.RequireDoctor, policy =>
                policy.RequireRole(Domain.Common.Enums.UserRole.Doctor.ToString()))
            .AddPolicy(Application.Common.Authorization.Policies.RequireReceptionist, policy =>
                policy.RequireRole(Domain.Common.Enums.UserRole.Receptionist.ToString()))
            .AddPolicy(Application.Common.Authorization.Policies.RequireSystemAdmin, policy =>
                policy.RequireRole(Domain.Common.Enums.UserRole.SystemAdmin.ToString()))
            .AddPolicy(Application.Common.Authorization.Policies.RequireStaffMember, policy =>
                policy.RequireRole(
                    Domain.Common.Enums.UserRole.Doctor.ToString(),
                    Domain.Common.Enums.UserRole.Receptionist.ToString(),
                    Domain.Common.Enums.UserRole.Nurse.ToString()))
            
            // Resource-based policies
            .AddPolicy(Application.Common.Authorization.Policies.SameClinic, policy =>
                policy.AddRequirements(new Application.Common.Authorization.Requirements.SameClinicRequirement()))
            
            // Combined policies
            .AddPolicy(Application.Common.Authorization.Policies.ManageStaff, policy =>
            {
                policy.RequireRole(Domain.Common.Enums.UserRole.ClinicOwner.ToString());
                policy.AddRequirements(new Application.Common.Authorization.Requirements.ClinicOwnerRequirement());
            })
            .AddPolicy(Application.Common.Authorization.Policies.ManagePatients, policy =>
            {
                policy.RequireRole(
                    Domain.Common.Enums.UserRole.Doctor.ToString(),
                    Domain.Common.Enums.UserRole.Receptionist.ToString(),
                    Domain.Common.Enums.UserRole.ClinicOwner.ToString());
                policy.AddRequirements(new Application.Common.Authorization.Requirements.SameClinicRequirement());
            })
            .AddPolicy(Application.Common.Authorization.Policies.ManageSubscription, policy =>
            {
                policy.RequireRole(Domain.Common.Enums.UserRole.ClinicOwner.ToString());
                policy.AddRequirements(new Application.Common.Authorization.Requirements.ClinicOwnerRequirement());
            })
            .AddPolicy(Application.Common.Authorization.Policies.ViewReports, policy =>
            {
                policy.RequireRole(
                    Domain.Common.Enums.UserRole.Doctor.ToString(),
                    Domain.Common.Enums.UserRole.ClinicOwner.ToString());
                policy.AddRequirements(new Application.Common.Authorization.Requirements.SameClinicRequirement());
            });

        // Core Services - Essential for Auth and Staff Inviting only
        services.AddHttpContextAccessor();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<ICookieService, CookieService>();

        // Options & Validation
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection("Smtp"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
