using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.RateLimiting;

/// <summary>
/// Named rate limit policies used across the API.
///
/// Algorithm choices:
///   SlidingWindow  → auth endpoints (per-IP, tight, no burst tolerance)
///   TokenBucket    → authenticated reads/writes and anon static data (burst-tolerant)
///   FixedWindow    → upload and one-time actions (hard hourly/daily caps)
///
/// Policy naming convention:
///   "auth-*"       → unauthenticated auth endpoints (SlidingWindow, per-IP, tight)
///   "user-reads"   → authenticated GET endpoints (TokenBucket, per-user, burst-tolerant)
///   "user-writes"  → authenticated write endpoints (TokenBucket, per-user, moderate)
///   "user-deletes" → authenticated delete endpoints (SlidingWindow, per-user, strict, no burst)
///   "user-upload"  → file upload (FixedWindow, per-user, hourly hard cap)
///   "user-once"    → one-time actions like onboarding (FixedWindow, per-user, daily hard cap)
///   "anon-static"  → anonymous static/reference data (TokenBucket, per-IP, burst-tolerant)
/// </summary>
public static class RateLimitPolicies
{
    public const string AuthLogin              = "auth-login";
    public const string AuthRegister           = "auth-register";
    public const string AuthForgotPassword     = "auth-forgot-password";
    public const string AuthResendVerification = "auth-resend-verification";
    public const string AuthResetPassword      = "auth-reset-password";
    public const string AuthConfirmEmail       = "auth-confirm-email";
    public const string AuthRefresh            = "auth-refresh";
    public const string AuthEnumeration        = "auth-enumeration";

    public const string UserReads   = "user-reads";
    public const string UserWrites  = "user-writes";
    public const string UserDeletes = "user-deletes";
    public const string UserUpload  = "user-upload";
    public const string UserOnce    = "user-once";

    public const string AnonStatic = "anon-static";
}

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Return 429 with Retry-After header
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString("0");
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { error = "Too many requests. Please slow down.", retryAfter = retryAfter.TotalSeconds },
                    ct);
            };

            // ── Unauthenticated auth endpoints (per-IP, tight) ────────────────

            // Login: 10 attempts per 15 minutes per IP
            options.AddSlidingWindowLimiter(RateLimitPolicies.AuthLogin, o =>
            {
                o.Window          = TimeSpan.FromMinutes(15);
                o.SegmentsPerWindow = 3;
                o.PermitLimit     = 10;
                o.QueueLimit      = 0;
                o.AutoReplenishment = true;
            });

            // Register: 5 per hour per IP
            options.AddSlidingWindowLimiter(RateLimitPolicies.AuthRegister, o =>
            {
                o.Window          = TimeSpan.FromHours(1);
                o.SegmentsPerWindow = 4;
                o.PermitLimit     = 5;
                o.QueueLimit      = 0;
                o.AutoReplenishment = true;
            });

            // Forgot password / resend verification: 3 per 15 min per IP
            options.AddSlidingWindowLimiter(RateLimitPolicies.AuthForgotPassword, o =>
            {
                o.Window          = TimeSpan.FromMinutes(15);
                o.SegmentsPerWindow = 3;
                o.PermitLimit     = 3;
                o.QueueLimit      = 0;
                o.AutoReplenishment = true;
            });

            options.AddSlidingWindowLimiter(RateLimitPolicies.AuthResendVerification, o =>
            {
                o.Window          = TimeSpan.FromMinutes(15);
                o.SegmentsPerWindow = 3;
                o.PermitLimit     = 3;
                o.QueueLimit      = 0;
                o.AutoReplenishment = true;
            });

            // Reset / confirm email: 5 per 15 min per IP
            options.AddSlidingWindowLimiter(RateLimitPolicies.AuthResetPassword, o =>
            {
                o.Window          = TimeSpan.FromMinutes(15);
                o.SegmentsPerWindow = 3;
                o.PermitLimit     = 5;
                o.QueueLimit      = 0;
                o.AutoReplenishment = true;
            });

            options.AddSlidingWindowLimiter(RateLimitPolicies.AuthConfirmEmail, o =>
            {
                o.Window          = TimeSpan.FromMinutes(15);
                o.SegmentsPerWindow = 3;
                o.PermitLimit     = 5;
                o.QueueLimit      = 0;
                o.AutoReplenishment = true;
            });

            // Token refresh: 30 per minute per IP
            options.AddSlidingWindowLimiter(RateLimitPolicies.AuthRefresh, o =>
            {
                o.Window          = TimeSpan.FromMinutes(1);
                o.SegmentsPerWindow = 2;
                o.PermitLimit     = 30;
                o.QueueLimit      = 0;
                o.AutoReplenishment = true;
            });

            // Email/username availability check: 20 per minute per IP
            options.AddSlidingWindowLimiter(RateLimitPolicies.AuthEnumeration, o =>
            {
                o.Window          = TimeSpan.FromMinutes(1);
                o.SegmentsPerWindow = 2;
                o.PermitLimit     = 20;
                o.QueueLimit      = 0;
                o.AutoReplenishment = true;
            });

            // ── Authenticated endpoints (per-user via JWT sub claim) ──────────

            // Reads: 200 per minute per user, burst of 30
            // TokenBucket: authenticated users legitimately burst (page load fires multiple GETs)
            options.AddPolicy(RateLimitPolicies.UserReads, context =>
                RateLimitPartition.GetTokenBucketLimiter(
                    GetUserId(context),
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit          = 200,
                        TokensPerPeriod     = 200,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        QueueLimit          = 0,
                        AutoReplenishment   = true,
                    }));

            // Writes: 60 per minute per user, burst of 10
            // TokenBucket: allows short bursts (e.g. saving multiple fields) without penalizing
            options.AddPolicy(RateLimitPolicies.UserWrites, context =>
                RateLimitPartition.GetTokenBucketLimiter(
                    GetUserId(context),
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit          = 60,
                        TokensPerPeriod     = 60,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        QueueLimit          = 0,
                        AutoReplenishment   = true,
                    }));

            // Deletes: 20 per minute per user — SlidingWindow (no burst tolerance for destructive ops)
            options.AddPolicy(RateLimitPolicies.UserDeletes, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetUserId(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 3,
                        PermitLimit       = 20,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // File upload: 10 per hour per user — FixedWindow (hourly hard cap)
            options.AddPolicy(RateLimitPolicies.UserUpload, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserId(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromHours(1),
                        PermitLimit       = 10,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // One-time actions (onboarding, accept invitation): 3 per day — FixedWindow (daily hard cap)
            options.AddPolicy(RateLimitPolicies.UserOnce, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserId(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromDays(1),
                        PermitLimit       = 3,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // ── Anonymous static/reference data (per-IP) ─────────────────────

            // TokenBucket: page load fetches countries+states+cities simultaneously (burst of 10)
            // 60 per minute sustained, burst up to 15
            options.AddPolicy(RateLimitPolicies.AnonStatic, context =>
                RateLimitPartition.GetTokenBucketLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit          = 15,
                        TokensPerPeriod     = 60,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        QueueLimit          = 0,
                        AutoReplenishment   = true,
                    }));
        });

        return services;
    }

    /// <summary>
    /// Partition key for authenticated endpoints.
    /// Uses the JWT sub (user ID) so limits are per-user, not per-IP.
    /// Falls back to IP if somehow called unauthenticated.
    /// </summary>
    private static string GetUserId(HttpContext context)
        => context.User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? context.Connection.RemoteIpAddress?.ToString()
           ?? "unknown";
}
