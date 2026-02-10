using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Common.Interfaces;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Options;
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
        // Caching
        services.AddMemoryCache();

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });
        
        // Unit of Work & Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Identity
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
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

        // JWT Authentication
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() 
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
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.Key)),
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Cookies["accessToken"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        // Core Services
        services.AddHttpContextAccessor();
        services.AddHttpClient<IGeoNamesService, GeoNamesService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();
        services.AddScoped<IPhoneValidationService, PhoneValidationService>();
        services.AddScoped<IUserRegistrationService, UserRegistrationService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IEmailSmtpClient, MailKitSmtpClient>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Configuration Options
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.Configure<CookieSettings>(configuration.GetSection("Cookie"));
        services.Configure<GeoNamesOptions>(configuration.GetSection(GeoNamesOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));

        // Background Services
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IDatabaseInitializationService, DatabaseInitializationService>();
        services.AddScoped<IComprehensiveSeedService, ComprehensiveSeedService>();
        services.AddHostedService<RefreshTokenCleanupService>();

        return services;
    }
}