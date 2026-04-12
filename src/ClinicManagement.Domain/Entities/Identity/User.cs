using ClinicManagement.Domain.Enums;
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
    public Gender Gender { get; set; }

    // Security enhancements (US-7)
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset? LastPasswordChangeAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
