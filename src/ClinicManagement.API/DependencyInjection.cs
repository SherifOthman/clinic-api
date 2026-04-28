using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.API.Authorization;
using ClinicManagement.API.Hangfire;
using ClinicManagement.API.Middleware;
using ClinicManagement.API.OpenApi;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Infrastructure.Options;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace ClinicManagement.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        AddCaching(services);
        AddAuthentication(services, configuration);
        AddAuthorization(services);
        AddOptions(services, configuration);
        AddCors(services, configuration);
        AddSwagger(services);

        services.AddRateLimiting();

        services.AddControllers();

        // Hangfire — skip in Testing environment (no SQL Server schema setup needed)
        var env = services.BuildServiceProvider().GetService<IWebHostEnvironment>();
        if (env?.IsEnvironment("Testing") != true)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing");

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout   = TimeSpan.FromMinutes(5),
                    QueuePollInterval            = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks           = true,
                }));

            services.AddHangfireServer(options => options.WorkerCount = 2);
        }

        return services;
    }

    private static void AddCaching(IServiceCollection services)
        => services.AddMemoryCache();

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.Section).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing");

        var googleOptions = configuration.GetSection(GoogleOAuthOptions.Section).Get<GoogleOAuthOptions>();

        services.AddAuthentication(options =>
        {
            // JWT is the default for API endpoints
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            // Cookie scheme is used only for the OAuth state/callback flow
            options.DefaultSignInScheme       = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            // Short-lived — only used to carry OAuth state between redirect and callback
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken            = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.Key)),
                ValidateIssuer           = true,
                ValidIssuer              = jwtOptions.Issuer,
                ValidateAudience         = true,
                ValidAudience            = jwtOptions.Audience,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero,
            };
        })
        .AddGoogle(options =>
        {
            options.ClientId     = googleOptions?.ClientId     ?? configuration["Google:ClientId"]     ?? "";
            options.ClientSecret = googleOptions?.ClientSecret ?? configuration["Google:ClientSecret"] ?? "";
            options.CallbackPath = "/api/auth/oauth/google/callback";
            options.Scope.Add("email");
            options.Scope.Add("profile");
            options.SaveTokens = false;
        });
    }

    private static void AddAuthorization(IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Dynamic policy provider — generates Permission:X policies on-demand.
        // Any new Permission enum value is automatically available without touching DI.
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        services.AddAuthorization(options =>
        {
            // ── Role-based policies ───────────────────────────────────────────
            // Roles are still included in JWT claims so these policies work.
            // Fine-grained access control uses [RequirePermission] instead.

            options.AddPolicy("RequireClinicOwner", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("ClinicId");
                policy.RequireRole(UserRoles.ClinicOwner);
            });

            options.AddPolicy("SuperAdmin", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(UserRoles.SuperAdmin);
            });

            // Permission:X policies are generated dynamically by PermissionPolicyProvider.
            // No foreach loop needed here.
        });
    }

    private static void AddOptions(IServiceCollection services, IConfiguration configuration)
    {
        // Application-level options (used by Application layer)
        services.Configure<AppOptions>(configuration.GetSection(AppOptions.Section));

        // Infrastructure-level options (used by Infrastructure layer only)
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Section));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.Section));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.Section));
        services.Configure<GeoNamesOptions>(configuration.GetSection(GeoNamesOptions.Section));
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.Section));
        services.Configure<CookieSettings>(configuration.GetSection(CookieSettings.Section));
        services.Configure<GoogleOAuthOptions>(configuration.GetSection(GoogleOAuthOptions.Section));
    }

    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            var corsOptions = configuration.GetSection(CorsOptions.Section).Get<CorsOptions>() ?? new CorsOptions();

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
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });
    }

    public static WebApplication UseAppConfigurations(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseStaticFiles();
        app.UseRouting();

        // Hangfire dashboard — before CORS, after routing so embedded assets are served
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = [new HangfireAuthorizationFilter(
                    app.Configuration["HangfireDashboardKey"])],
            });
        }

        app.UseCors("AllowAll");
        app.UseRateLimiter();
        app.UseMiddleware<CookieTokenMiddleware>(); // inject cookie access token as Bearer header
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Clinic Management API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .AddPreferredSecuritySchemes("Bearer");
        });

        return app;
    }
}
