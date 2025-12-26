using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int? ReceptionistId { get; set; }
    public int? BranchId { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Receptionist? Receptionist { get; set; }
    public virtual ClinicBranch? Branch { get; set; }
}
