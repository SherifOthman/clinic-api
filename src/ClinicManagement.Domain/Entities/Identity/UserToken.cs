using System.Security.Cryptography;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Stores short-lived OTP codes for email confirmation and password reset.
///
/// Design decisions:
/// - 6-digit numeric OTP — works on web, mobile, and any future client
/// - Stored as a SHA-256 hash — a DB breach exposes only hashes
/// - One active token per user per type — old tokens are invalidated on new request
/// - 15-minute expiry for email confirmation, 10-minute expiry for password reset
/// - IsUsed flag prevents replay attacks (token can only be consumed once)
/// </summary>
public class UserToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string TokenType { get; set; } = string.Empty;

    /// <summary>SHA-256 hex hash of the raw 6-digit OTP. Never the raw value.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsExpired(DateTimeOffset now) => now > ExpiresAt;
    public bool IsValid(DateTimeOffset now) => !IsUsed && !IsExpired(now);

    // ── Domain factory ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new OTP token.
    /// Returns the entity (hash stored) and the raw 6-digit code to send to the user.
    /// </summary>
    public static (UserToken Entity, string RawOtp) Create(
        Guid userId, string tokenType, TimeSpan expiry)
    {
        // Cryptographically random 6-digit code: 000000–999999
        var raw = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        var entity = new UserToken
        {
            UserId    = userId,
            TokenType = tokenType,
            TokenHash = Hash(raw),
            ExpiresAt = DateTimeOffset.UtcNow.Add(expiry),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        return (entity, raw);
    }

    /// <summary>Marks the token as used so it cannot be replayed.</summary>
    public void MarkUsed()
    {
        IsUsed  = true;
        UsedAt  = DateTimeOffset.UtcNow;
    }

    /// <summary>SHA-256 hex hash of the raw OTP string.</summary>
    public static string Hash(string rawOtp)
        => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawOtp)));
}

public static class TokenTypes
{
    public const string EmailConfirmation = "EmailConfirmation";
    public const string PasswordReset     = "PasswordReset";
}
