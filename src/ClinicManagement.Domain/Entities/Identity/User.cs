using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
    }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }

    // Security enhancements (US-7)
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastPasswordChangeAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
