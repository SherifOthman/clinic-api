using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Doctor : AuditableEntity
{
    public Guid StaffId { get; init; }
    public Guid? SpecializationId { get; set; }

    /// <summary>
    /// When false, the doctor cannot edit their own working days or visit types.
    /// Only the clinic owner can modify them. Defaults to true (doctor can self-manage).
    /// </summary>
    public bool CanSelfManageSchedule { get; set; } = true;

    // Navigation properties
    public Staff Staff { get; set; } = null!;
    public Specialization? Specialization { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorWorkingDay> WorkingDays { get; set; } = new List<DoctorWorkingDay>();
    public ICollection<DoctorVisitType> VisitTypes { get; set; } = new List<DoctorVisitType>();
}
