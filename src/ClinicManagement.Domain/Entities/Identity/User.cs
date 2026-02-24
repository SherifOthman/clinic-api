namespace ClinicManagement.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserName { get; set; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsEmailConfirmed { get; set; }

    // Security enhancements (US-7)
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEndDate { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastPasswordChangeAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
