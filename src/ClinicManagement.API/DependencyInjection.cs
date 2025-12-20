using Asp.Versioning;
using ClinicManagement.API.Extensions;
using ClinicManagement.API.Middleware;
using ClinicManagement.API.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Threading.RateLimiting;

namespace ClinicManagement.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        
        // Add HttpContextAccessor for cookie service
        services.AddHttpContextAccessor();

        // Add Memory Cache
        services.AddMemoryCache();

        // Add JWT Cookie Middleware with options pattern (using same Jwt section as TokenService)
        services.AddJwtCookieMiddleware(options =>
        {
            var jwtSection = configuration.GetSection("Jwt");
            options.Key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            options.Issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            options.Audience = jwtSection["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            
            // Use middleware-specific settings from JwtMiddleware section if available, otherwise use defaults
            var middlewareSection = configuration.GetSection("JwtMiddleware");
            options.ClockSkew = middlewareSection.GetValue<TimeSpan?>("ClockSkew") ?? TimeSpan.Zero;
            options.ValidateIssuer = middlewareSection.GetValue<bool?>("ValidateIssuer") ?? true;
            options.ValidateAudience = middlewareSection.GetValue<bool?>("ValidateAudience") ?? true;
            options.ValidateIssuerSigningKey = middlewareSection.GetValue<bool?>("ValidateIssuerSigningKey") ?? true;
            options.EnableAutoRefresh = middlewareSection.GetValue<bool?>("EnableAutoRefresh") ?? true;
            options.ClearCookiesOnInvalidToken = middlewareSection.GetValue<bool?>("ClearCookiesOnInvalidToken") ?? true;
            options.EnableDetailedLogging = middlewareSection.GetValue<bool?>("EnableDetailedLogging") ?? false;
            options.RefreshBufferTime = middlewareSection.GetValue<TimeSpan?>("RefreshBufferTime") ?? TimeSpan.FromMinutes(5);
            
            // Public paths from middleware section or use defaults
            var publicPaths = middlewareSection.GetSection("PublicPaths").Get<string[]>();
            if (publicPaths != null && publicPaths.Length > 0)
            {
                options.PublicPaths = publicPaths;
            }
        });

        // Add JWT configuration validation to ensure TokenService and Middleware are synchronized
        services.AddJwtConfigurationValidation();

        // Add API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        }).AddMvc();

        // Add .NET Rate Limiting
        services.AddRateLimiter(options =>
        {
            // Fixed window rate limiter for general API
            options.AddFixedWindowLimiter("fixed", opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            // Sliding window for authenticated users
            options.AddSlidingWindowLimiter("sliding", opt =>
            {
                opt.PermitLimit = 1000;
                opt.Window = TimeSpan.FromHours(1);
                opt.SegmentsPerWindow = 6;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 20;
            });

            // Concurrency limiter for resource-intensive operations
            options.AddConcurrencyLimiter("concurrency", opt =>
            {
                opt.PermitLimit = 50;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            // Policy per endpoint
            options.AddPolicy("api", context =>
            {
                var clinicId = context.User?.FindFirst("ClinicId")?.Value;
                
                if (!string.IsNullOrEmpty(clinicId))
                {
                    // Authenticated users get sliding window
                    return RateLimitPartition.GetSlidingWindowLimiter(clinicId, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 1000,
                        Window = TimeSpan.FromHours(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 20
                    });
                }

                // Anonymous users get fixed window by IP
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                
                TimeSpan? retryAfter = null;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue))
                {
                    retryAfter = retryAfterValue;
                    context.HttpContext.Response.Headers.RetryAfter = retryAfterValue.TotalSeconds.ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    message = "Too many requests. Please try again later.",
                    retryAfter = retryAfter?.TotalSeconds
                }, cancellationToken);
            };
        });

        // Add Swagger - Cookie-based authentication (no Bearer tokens)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Clinic Management API",
                Version = "v1",
                Description = "API for managing clinic operations. Authentication is handled via secure HttpOnly cookies."
            });

            // No security definitions - authentication is handled via HttpOnly cookies
            // Users must login through the frontend to get authenticated cookies
        });

        // Add CORS for multiple frontend domains
        var allowedOrigins = new[] 
        { 
            "http://localhost:5173",  // React dev server
            "http://localhost:3000",  // Next.js dev server
            "https://clinic-dashboard.vercel.app", // React production
            "https://clinic-website.vercel.app"    // Next.js production
        }; 
        
        services.AddCors(options =>
        {
            options.AddPolicy("SecureCors", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials(); // Required for HttpOnly cookies
                // Removed SetIsOriginAllowedToReturnTrue() - security risk in production
            });
        });

        return services;
    }

    public static WebApplication UseAppConfigurations(this WebApplication app)
    {
        // Serilog request logging
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                
                var clinicId = httpContext.User?.FindFirst("ClinicId")?.Value;
                if (!string.IsNullOrEmpty(clinicId))
                {
                    diagnosticContext.Set("ClinicId", clinicId);
                }
            };
        });

        // Global exception middleware should be first
        app.UseMiddleware<GlobalExceptionMiddleware>();
        
        // .NET Rate Limiting
        app.UseRateLimiter();
        
        // JWT Cookie middleware for automatic token refresh (using options pattern)
        app.UseJwtCookieMiddleware();
        
        // Middleware removed for minimal version
        
        // Configure Swagger
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinic Management API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Clinic Management API";
        });

        // Only use HTTPS redirection in production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        
        app.UseCors("SecureCors");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Health checks removed for ultra minimal version

        return app;
    }
}
