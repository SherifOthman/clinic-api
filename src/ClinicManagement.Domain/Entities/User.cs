using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public bool HasAccount { get; set; }
    public bool IsSystemAdmin { get; set; }
    public string? City { get; set; }
    public int? PatientId { get; set;  }
    public Patient? Patient { get; set; }
    public int? StaffId { get; set; }
    public Staff? Staff { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // New relationships for visits and measurements
    public virtual ICollection<Visit> VisitsAsDoctor { get; set; } = new List<Visit>();
    public virtual ICollection<PatientMeasurement> MeasurementsTaken { get; set; } = new List<PatientMeasurement>();
}
