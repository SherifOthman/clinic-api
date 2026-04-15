using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity, INoAuditLog
{
    public string AppointmentNumber { get; set; } = null!;
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid AppointmentTypeId { get; set; }
    public DateTimeOffset AppointmentDate { get; set; }
    public short QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; }
    public Guid? InvoiceId { get; set; }
}
