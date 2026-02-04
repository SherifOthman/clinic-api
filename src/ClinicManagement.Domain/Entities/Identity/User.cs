using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Identity user for authentication only.
/// Business logic and roles are handled through ASP.NET Identity Claims/Roles.
/// </summary>
public class User : IdentityUser<Guid>
{   
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public string? ProfileImageUrl { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
