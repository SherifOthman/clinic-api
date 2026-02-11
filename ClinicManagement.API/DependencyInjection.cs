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
        
        // Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "database",
                tags: new[] { "db", "sql", "ready" });

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
        // Note: AddOpenApi is not available in .NET 10 preview yet
        // services.AddOpenApi(options =>
        // {
        //     options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        // });

        return services;
    }

    public static WebApplication UseAppConfigurations(this WebApplication app)
    {
        // Global exception middleware handles all unhandled exceptions
        app.UseMiddleware<GlobalExceptionMiddleware>();

        //if (app.Environment.IsDevelopment())
        //{
            // Map OpenAPI document endpoint
            // Note: MapOpenApi is not available in .NET 10 preview yet
            // app.MapOpenApi();
            
            // Map Scalar UI (modern, beautiful API documentation)
            // Note: MapScalarApiReference is not available in .NET 10 preview yet
            // app.MapScalarApiReference(options =>
            // {
            //     options
            //         .WithTitle("Clinic Management API")
            //         .WithTheme(ScalarTheme.Purple)
            //         .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            // });
        //}

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
        
        // Map Health Check Endpoints
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        duration = e.Value.Duration.TotalMilliseconds
                    }),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                });
                await context.Response.WriteAsync(result);
            }
        });
        
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });
        
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false // Just checks if the app is running
        });

        return app;
    }
}
