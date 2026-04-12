namespace ClinicManagement.Domain.Entities;

public class UserToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string TokenType { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public bool IsExpired(DateTimeOffset now) => now > ExpiresAt;
    public bool IsValid(DateTimeOffset now) => !IsUsed && !IsExpired(now);
}

public static class TokenTypes
{
    public const string EmailConfirmation = "EmailConfirmation";
    public const string PasswordReset = "PasswordReset";
}
