using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Authentication identity — email, password, tokens.
/// Personal data (name, gender, photo) is being migrated to Person.
/// PersonId links this User to their Person record.
/// </summary>
public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
    }

    // ── Kept during migration — will be removed once Person is fully adopted ──
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public Gender Gender { get; set; }

    // Security
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset? LastPasswordChangeAt { get; set; }

    /// <summary>
    /// Links this User to their Person record.
    /// Nullable during migration — will become required once all users have a Person.
    /// </summary>
    public Guid? PersonId { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    // Navigation
    public Person? Person { get; set; }
}
