using ClinicManagement.API.Infrastructure.Middleware;
using ClinicManagement.API.Infrastructure.Services;
using ClinicManagement.API.Infrastructure.Data;
using ClinicManagement.API.Common.Options;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scalar.AspNetCore;

namespace ClinicManagement.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();

        // Caching
        services.AddMemoryCache();
        services.AddOutputCache(options =>
        {
            // Default policy: no caching
            options.AddBasePolicy(builder => builder.NoCache());
            
            // Reference data policy: cache for 1 hour
            options.AddPolicy("ReferenceData", builder => 
                builder.Expire(TimeSpan.FromHours(1)));
            
            // Location data policy: cache for 24 hours (rarely changes)
            options.AddPolicy("LocationData", builder => 
                builder.Expire(TimeSpan.FromHours(24)));
        });

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

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

        // Core Services - using concrete classes (no interfaces)
        services.AddHttpClient<GeoNamesService>();
        services.AddScoped<DateTimeProvider>();
        services.AddScoped<UserManagementService>();
        services.AddScoped<EmailConfirmationService>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<TokenService>();
        services.AddScoped<CookieService>();
        services.AddScoped<CurrentUserService>();
        services.AddScoped<CodeGeneratorService>();
        services.AddScoped<PhoneValidationService>();
        services.AddScoped<UserRegistrationService>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<MailKitSmtpClient>();
        services.AddScoped<LocalFileStorageService>();

        // Background Services
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<DatabaseInitializationService>();
        services.AddScoped<ComprehensiveSeedService>();
        services.AddHostedService<RefreshTokenCleanupService>();

        // .NET 10 Native Validation
        services.AddValidation();

        // Options
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        services.Configure<GeoNamesOptions>(configuration.GetSection("GeoNames"));
        services.Configure<CorsOptions>(configuration.GetSection("Cors"));
        services.Configure<CookieSettings>(configuration.GetSection("Cookie"));

        services.AddCors(options =>
        {
            var corsOptions = configuration.GetSection("Cors").Get<CorsOptions>() ?? new CorsOptions();
            
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // OpenAPI with Scalar (modern alternative to Swagger)
        services.AddOpenApi();

        return services;
    }

    public static WebApplication UseAppConfigurations(this WebApplication app)
    {
        // Global exception middleware handles all unhandled exceptions
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // Enable static file serving for uploaded files
        app.UseStaticFiles();

        app.UseCors("AllowAll");
        app.UseOutputCache(); // Add output caching middleware
        app.UseMiddleware<JwtCookieMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        
        // Map Controllers (old approach - being phased out)
        app.MapControllers();

        // Map Minimal API Endpoints (new approach - Vertical Slice Architecture)
        app.MapEndpoints();
        
        // Map OpenAPI document and Scalar UI
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Clinic Management API")
                .WithTheme(ScalarTheme.Purple)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
        
        // Simple Health Check Endpoints (no external dependencies)
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "Clinic Management API"
        }))
        .WithName("HealthCheck")
        .WithTags("Health")
        .AllowAnonymous();
        
        app.MapGet("/health/ready", () => Results.Ok(new
        {
            status = "Ready",
            timestamp = DateTime.UtcNow
        }))
        .WithName("ReadinessCheck")
        .WithTags("Health")
        .AllowAnonymous();
        
        app.MapGet("/health/live", () => Results.Ok(new
        {
            status = "Live",
            timestamp = DateTime.UtcNow
        }))
        .WithName("LivenessCheck")
        .WithTags("Health")
        .AllowAnonymous();

        return app;
    }
}
