using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class MedicalVisit : AuditableEntity, INoAuditLog
{
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid AppointmentId { get; set; }
    public string? Diagnosis { get; set; }
}
