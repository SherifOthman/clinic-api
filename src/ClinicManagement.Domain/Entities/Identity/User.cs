using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Authentication identity — credentials, tokens, and personal profile in one place.
/// Removing the Person split eliminates a mandatory JOIN on every auth operation.
/// </summary>
public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
    }

    // Profile
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Security
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset? LastPasswordChangeAt { get; set; }
}
