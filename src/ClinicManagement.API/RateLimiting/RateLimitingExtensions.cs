using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.RateLimiting;

/// <summary>
/// Named rate limit policies used across the API.
///
/// Algorithm choices:
///   SlidingWindow  → auth endpoints (per-device, tight, no burst tolerance)
///   TokenBucket    → authenticated writes and anon static data (burst-tolerant)
///   FixedWindow    → upload and one-time actions (hard hourly/daily caps)
///   Concurrency    → authenticated reads (prevents resource exhaustion from concurrent requests)
///
/// Policy naming convention:
///   "auth-*"       → unauthenticated auth endpoints (SlidingWindow, per-device, tight)
///   "user-reads"   → authenticated GET endpoints (Concurrency, per-user, prevents overload)
///   "user-writes"  → authenticated write endpoints (TokenBucket, per-user, moderate)
///   "user-deletes" → authenticated delete endpoints (SlidingWindow, per-user, strict, no burst)
///   "user-upload"  → file upload (FixedWindow, per-user, hourly hard cap)
///   "user-once"    → one-time actions like onboarding (FixedWindow, per-user, daily hard cap)
///   "anon-static"  → anonymous static/reference data (TokenBucket, per-IP, burst-tolerant)
/// </summary>
public static class RateLimitPolicies
{
    public const string AuthLogin = "auth-login";
    public const string AuthRegister = "auth-register";
    public const string AuthForgotPassword = "auth-forgot-password";
    public const string AuthResendVerification = "auth-resend-verification";
    public const string AuthResetPassword = "auth-reset-password";
    public const string AuthConfirmEmail = "auth-confirm-email";
    public const string AuthRefresh = "auth-refresh";
    public const string AuthEnumeration = "auth-enumeration";

    public const string UserReads = "user-reads";
    public const string UserWrites = "user-writes";
    public const string UserDeletes = "user-deletes";
    public const string UserUpload = "user-upload";
    public const string UserOnce = "user-once";

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

            // Login: 10 attempts per 15 minutes per device
            options.AddPolicy(RateLimitPolicies.AuthLogin, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // Register: 5 per hour per device
            options.AddPolicy(RateLimitPolicies.AuthRegister, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromHours(1),
                        SegmentsPerWindow = 4,
                        PermitLimit = 5,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // Forgot password: 3 per 15 min per device
            options.AddPolicy(RateLimitPolicies.AuthForgotPassword, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        PermitLimit = 3,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // Resend verification: 3 per 15 min per device
            options.AddPolicy(RateLimitPolicies.AuthResendVerification, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        PermitLimit = 3,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // Reset / confirm email: 5 per 15 min per device
            options.AddPolicy(RateLimitPolicies.AuthResetPassword, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        PermitLimit = 5,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            options.AddPolicy(RateLimitPolicies.AuthConfirmEmail, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        PermitLimit = 5,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // Token refresh: 30 per minute per device
            options.AddPolicy(RateLimitPolicies.AuthRefresh, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 2,
                        PermitLimit = 30,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // Email/username availability check: 20 per minute per device
            options.AddPolicy(RateLimitPolicies.AuthEnumeration, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 2,
                        PermitLimit = 20,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // ── Authenticated endpoints (per-user via JWT sub claim) ──────────

            // Reads: 5 concurrent requests per user — Concurrency (prevents resource exhaustion from multiple tabs)
            options.AddPolicy(RateLimitPolicies.UserReads, context =>
                RateLimitPartition.GetConcurrencyLimiter(
                    GetUserId(context),
                    _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 5,
                        QueueLimit = 10,
                    }));

            // Writes: 60 per minute per user — TokenBucket allows short bursts (saving multiple fields)
            options.AddPolicy(RateLimitPolicies.UserWrites, context =>
                RateLimitPartition.GetTokenBucketLimiter(
                    GetUserId(context),
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 60,
                        TokensPerPeriod = 60,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // Deletes: 20 per minute per user — SlidingWindow (no burst tolerance for destructive ops)
            options.AddPolicy(RateLimitPolicies.UserDeletes, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetUserId(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 3,
                        PermitLimit = 20,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // File upload: 10 per hour per user — FixedWindow (hourly hard cap)
            options.AddPolicy(RateLimitPolicies.UserUpload, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserId(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromHours(1),
                        PermitLimit = 10,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // One-time actions (onboarding, accept invitation): 3 per day — FixedWindow (daily hard cap)
            options.AddPolicy(RateLimitPolicies.UserOnce, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserId(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromDays(1),
                        PermitLimit = 3,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            // ── Anonymous static/reference data (per-IP) ─────────────────────

            // TokenBucket: page load fetches countries+states+cities simultaneously (burst of 15)
            // 60 per minute sustained, burst up to 15
            options.AddPolicy(RateLimitPolicies.AnonStatic, context =>
                RateLimitPartition.GetTokenBucketLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 15,
                        TokensPerPeriod = 60,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true,
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

    /// <summary>
    /// Generates a device fingerprint for rate limiting unauthenticated requests.
    /// Combines IP with User-Agent and Accept-Language to differentiate devices
    /// within the same network while avoiding overly broad blocking.
    /// </summary>
    private static string GetDeviceFingerprint(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        return $"{ip}|{userAgent}|{acceptLanguage}";
    }
}
