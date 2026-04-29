using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.RateLimiting;

/// <summary>
/// Rate limit policies — practical limits based on real usage patterns.
///
/// Guiding principles:
///   - Auth endpoints: tight, per-device fingerprint (IP + UA) to prevent brute-force
///   - Authenticated reads: generous sliding window per-user (multiple tabs, fast navigation)
///   - Authenticated writes: moderate token bucket per-user (burst-tolerant for form saves)
///   - Destructive ops: stricter sliding window (no burst tolerance)
///   - One-time actions: hard daily cap (onboarding, invitation accept)
///   - Public/anon: per-IP token bucket (burst-tolerant for page loads)
///   - Contact form: separate from UserOnce — public, per-IP, a few per hour
/// </summary>
public static class RateLimitPolicies
{
    // Auth (unauthenticated)
    public const string AuthLogin              = "auth-login";
    public const string AuthRegister           = "auth-register";
    public const string AuthForgotPassword     = "auth-forgot-password";
    public const string AuthResendVerification = "auth-resend-verification";
    public const string AuthResetPassword      = "auth-reset-password";
    public const string AuthConfirmEmail       = "auth-confirm-email";
    public const string AuthRefresh            = "auth-refresh";
    public const string AuthEnumeration        = "auth-enumeration";

    // Authenticated
    public const string UserReads   = "user-reads";
    public const string UserWrites  = "user-writes";
    public const string UserDeletes = "user-deletes";
    public const string UserUpload  = "user-upload";
    public const string UserOnce    = "user-once";    // onboarding, accept-invitation
    public const string UserLogout  = "user-logout";  // logout — very generous

    // Public / anonymous
    public const string AnonStatic  = "anon-static";  // reference data, pricing, stats
    public const string AnonContact = "anon-contact"; // contact form — per-IP, hourly cap
}

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        retryAfter.TotalSeconds.ToString("0");
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { error = "Too many requests. Please slow down.", retryAfter = retryAfter.TotalSeconds },
                    ct);
            };

            // ── Auth endpoints (unauthenticated, per-device fingerprint) ──────

            // Login: 30 attempts per 15 min — enough for normal use, blocks brute-force
            // A real user might mistype their password a few times; 30 is generous
            options.AddPolicy(RateLimitPolicies.AuthLogin, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        PermitLimit       = 30,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // Register: 10 per hour — prevents mass account creation
            options.AddPolicy(RateLimitPolicies.AuthRegister, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromHours(1),
                        SegmentsPerWindow = 4,
                        PermitLimit       = 10,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // Forgot password: 5 per 15 min — user might try multiple emails
            options.AddPolicy(RateLimitPolicies.AuthForgotPassword, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        PermitLimit       = 5,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // Resend verification: 5 per 15 min
            options.AddPolicy(RateLimitPolicies.AuthResendVerification, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        PermitLimit       = 5,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // Reset / confirm email: 10 per 15 min — link clicks from email
            options.AddPolicy(RateLimitPolicies.AuthResetPassword, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        PermitLimit       = 10,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            options.AddPolicy(RateLimitPolicies.AuthConfirmEmail, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        PermitLimit       = 10,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // Token refresh: 60 per minute — silent middleware refresh fires on every expired request
            options.AddPolicy(RateLimitPolicies.AuthRefresh, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        PermitLimit       = 60,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // Email/username availability check: 60 per minute — fires on every keystroke (debounced)
            options.AddPolicy(RateLimitPolicies.AuthEnumeration, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetDeviceFingerprint(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        PermitLimit       = 60,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // ── Authenticated endpoints (per-user JWT sub) ────────────────────

            // Reads: 300 per minute per user — generous sliding window
            // Multiple browser tabs, fast navigation, dashboard polling all need headroom
            // Concurrency limiter was wrong here — it blocked parallel tab loads
            options.AddPolicy(RateLimitPolicies.UserReads, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetUserId(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        PermitLimit       = 300,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // Writes: 120 per minute per user — token bucket allows short bursts
            // (saving a form, bulk-updating patients, inviting multiple staff)
            options.AddPolicy(RateLimitPolicies.UserWrites, ctx =>
                RateLimitPartition.GetTokenBucketLimiter(
                    GetUserId(ctx),
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit          = 30,   // burst: up to 30 at once
                        TokensPerPeriod     = 30,   // refill 30 every 15s = 120/min sustained
                        ReplenishmentPeriod = TimeSpan.FromSeconds(15),
                        QueueLimit          = 0,
                        AutoReplenishment   = true,
                    }));

            // Deletes: 30 per minute per user — stricter, no burst
            // Bulk patient deletion is a real use case but should be deliberate
            options.AddPolicy(RateLimitPolicies.UserDeletes, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetUserId(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        PermitLimit       = 30,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // File upload: 20 per hour per user — profile images, documents
            options.AddPolicy(RateLimitPolicies.UserUpload, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserId(ctx),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromHours(1),
                        PermitLimit       = 20,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // One-time actions: 5 per day — onboarding complete, accept invitation
            // These are genuinely once-per-lifetime actions; 5 allows retries on error
            options.AddPolicy(RateLimitPolicies.UserOnce, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserId(ctx),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromDays(1),
                        PermitLimit       = 5,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // Logout: 20 per minute — should never be blocked in practice
            options.AddPolicy(RateLimitPolicies.UserLogout, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetUserId(ctx),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 2,
                        PermitLimit       = 20,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));

            // ── Anonymous / public endpoints (per-IP) ─────────────────────────

            // Static reference data: pricing, locations, stats, subscription plans
            // Page load fetches several simultaneously — burst of 30, 120/min sustained
            options.AddPolicy(RateLimitPolicies.AnonStatic, ctx =>
                RateLimitPartition.GetTokenBucketLimiter(
                    ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit          = 30,   // burst: 30 simultaneous page-load requests
                        TokensPerPeriod     = 30,   // refill 30 every 15s = 120/min sustained
                        ReplenishmentPeriod = TimeSpan.FromSeconds(15),
                        QueueLimit          = 0,
                        AutoReplenishment   = true,
                    }));

            // Contact form: 5 per hour per IP — prevents spam, allows genuine retries
            options.AddPolicy(RateLimitPolicies.AnonContact, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromHours(1),
                        PermitLimit       = 5,
                        QueueLimit        = 0,
                        AutoReplenishment = true,
                    }));
        });

        return services;
    }

    private static string GetUserId(HttpContext context)
        => context.User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? context.Connection.RemoteIpAddress?.ToString()
           ?? "unknown";

    private static string GetDeviceFingerprint(HttpContext context)
    {
        var ip          = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent   = context.Request.Headers.UserAgent.ToString();
        var acceptLang  = context.Request.Headers.AcceptLanguage.ToString();
        return $"{ip}|{userAgent}|{acceptLang}";
    }
}
