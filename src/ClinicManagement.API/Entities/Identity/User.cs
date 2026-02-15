using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Identity user for authentication only.
/// Pure identity - no clinic-specific data.
/// Roles are managed through ASP.NET Identity.
/// Clinic membership is managed through Staff table.
/// </summary>
public class User : IdentityUser<Guid>
{   
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }

    // Navigation properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    /// <summary>
    /// Staff memberships at various clinics (user can work at multiple clinics)
    /// </summary>
    public virtual ICollection<Staff> StaffMemberships { get; set; } = new List<Staff>();
    
    /// <summary>
    /// Clinics owned by this user (if user has ClinicOwner role)
    /// </summary>
    public virtual ICollection<Clinic> OwnedClinics { get; set; } = new List<Clinic>();
}
