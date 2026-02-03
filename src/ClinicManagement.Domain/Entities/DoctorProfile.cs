using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Doctor-specific profile information.
/// Links to Staff (must be doctor role) and Specialization.
/// </summary>
public class DoctorProfile : BaseEntity
{
   public Guid StaffId { get; set; }
   public Staff Staff { get; set; } = null!;
   
   public Guid SpecializationId { get; set; }
   public Specialization Specialization { get; set; } = null!;
   
   // Additional doctor information
   public string? LicenseNumber { get; set; }
   public DateTime? LicenseExpiryDate { get; set; }
   public int? YearsOfExperience { get; set; }
   public string? Qualifications { get; set; }
   public string? Biography { get; set; }
}
