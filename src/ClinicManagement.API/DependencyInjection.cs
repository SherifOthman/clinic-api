using System.Text;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Common.Options;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using ClinicManagement.API.Infrastructure.Middleware;
using ClinicManagement.API.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace ClinicManagement.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment? environment = null)
    {
        // HTTP Context
        services.AddHttpContextAccessor();

        // Caching
        AddCaching(services);

        // Database
        AddDatabase(services, configuration, environment);

        // Identity & Authentication
        AddIdentity(services);
        AddAuthentication(services, configuration);
        services.AddAuthorization();

        // Application Services
        AddApplicationServices(services);

        // Configuration Options
        AddOptions(services, configuration);

        // CORS
        AddCors(services, configuration);

        // API Documentation
        AddSwagger(services);

        return services;
    }

    private static void AddCaching(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder => builder.NoCache());
            options.AddPolicy("ReferenceData", builder => builder.Expire(TimeSpan.FromHours(1)));
            options.AddPolicy("LocationData", builder => builder.Expire(TimeSpan.FromHours(24)));
        });
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment? environment)
    {
        // Skip registration in Testing environment (handled by test factory)
        if (environment?.EnvironmentName != "Testing")
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            });
        }
    }

    private static void AddIdentity(IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Sign-in settings
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
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
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        // External API Services
        services.AddHttpClient<GeoNamesService>();

        // Core Services
        services.AddScoped<DateTimeProvider>();
        services.AddScoped<CodeGeneratorService>();
        services.AddScoped<PhoneValidationService>();

        // User Management Services
        services.AddScoped<UserRegistrationService>();
        services.AddScoped<CurrentUserService>();

        // Authentication Services
        services.AddScoped<AuthenticationService>();
        services.AddScoped<TokenService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<CookieService>();

        // Email Services
        services.AddScoped<EmailConfirmationService>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<MailKitSmtpClient>();

        // File Storage Services
        services.AddScoped<LocalFileStorageService>();

        // Database Services
        services.AddScoped<DatabaseInitializationService>();
        services.AddScoped<ComprehensiveSeedService>();

        // Background Services
        services.AddHostedService<RefreshTokenCleanupService>();
    }

    private static void AddOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        services.Configure<GeoNamesOptions>(configuration.GetSection("GeoNames"));
        services.Configure<CorsOptions>(configuration.GetSection("Cors"));
        services.Configure<CookieSettings>(configuration.GetSection("Cookie"));
    }

    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
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
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Clinic Management API",
                Version = "v1",
                Description = "API for managing clinic operations including appointments, patients, billing, and more."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
            options.UseAllOfToExtendReferenceSchemas();
        });
    }

    public static WebApplication UseAppConfigurations(this WebApplication app)
    {
        // Exception Handling
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // Static Files
        app.UseStaticFiles();

        // CORS
        app.UseCors("AllowAll");

        // Caching
        app.UseOutputCache();

        // Authentication & Authorization
        app.UseMiddleware<JwtCookieMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        // Endpoints
        app.MapEndpoints();

        // API Documentation
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinic Management API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
