using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Identity user for authentication only.
/// Business logic and roles are handled through ASP.NET Identity Claims/Roles.
/// Links to Patient and Staff entities for business data.
/// </summary>
public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Optional link to patient entity
    public Guid? PatientId { get; set; }
    public Patient? Patient { get; set; }
    
    // Navigation properties for clinical operations
    public virtual ICollection<Visit> VisitsAsDoctor { get; set; } = new List<Visit>();
    public virtual ICollection<VisitMeasurement> MeasurementsTaken { get; set; } = new List<VisitMeasurement>();
    public virtual ICollection<Staff> StaffRoles { get; set; } = new List<Staff>();
}
