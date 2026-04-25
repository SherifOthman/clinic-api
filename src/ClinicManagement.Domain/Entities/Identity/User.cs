using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Authentication identity — email, password, tokens only.
/// All personal data (name, gender, photo) lives on Person.
/// </summary>
public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
    }

    // Security
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset? LastPasswordChangeAt { get; set; }

    public Guid PersonId { get; set; }
    

    // Navigation
    public Person Person { get; set; } = null!;
}
