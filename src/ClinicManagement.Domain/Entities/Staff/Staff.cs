using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents a staff member's association with a clinic.
/// All clinic-related users (ClinicOwner, Doctor, Receptionist) have a Staff record.
/// Role is determined by ASP.NET Identity Roles, not by a type field.
/// </summary>
public class Staff : TenantEntity
{
    /// <summary>
    /// Reference to the User (identity)
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Whether this staff member is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Date when the staff member was hired/added to the clinic
    /// </summary>
    public DateTime HireDate { get; set; }
    
    /// <summary>
    /// Date when the staff member left the clinic (if applicable)
    /// </summary>
    public DateTime? TerminationDate { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Clinic Clinic { get; set; } = null!;
    
    /// <summary>
    /// Doctor-specific profile (only populated if user has Doctor role)
    /// </summary>
    public virtual DoctorProfile? DoctorProfile { get; set; }
}
