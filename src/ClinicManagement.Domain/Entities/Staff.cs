using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Staff belongs to ONE clinic only.
/// Staff roles: Doctor, Receptionist, ClinicOwner
/// SystemAdmin is NOT a staff role - handled via ASP.NET Identity roles.
/// </summary>
public class Staff : BaseEntity
{
   public Guid UserId { get; set; }
   public User User { get; set; } = null!;
   public Guid ClinicId { get; set; }
   public Clinic Clinic { get; set; } = null!;
   public StaffRole Role { get; set; }
   public bool IsActive { get; set; } = true;
   
   // Doctor-specific data
   public DoctorProfile? DoctorProfile { get; set; }
   
   // Navigation properties
   public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
   public ICollection<DoctorMeasurement> DoctorMeasurements { get; set; } = new List<DoctorMeasurement>();
   
   // Helper methods
   public bool IsDoctor() => Role == StaffRole.Doctor;
   public bool IsReceptionist() => Role == StaffRole.Receptionist;
   public bool IsClinicOwner() => Role == StaffRole.ClinicOwner;
}
