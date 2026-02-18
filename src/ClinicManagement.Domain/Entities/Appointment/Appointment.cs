using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity
{
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid AppointmentTypeId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public short QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; }
    public Guid? InvoiceId { get; set; }

    public bool IsPending => Status == AppointmentStatus.Pending;
    public bool IsConfirmed => Status == AppointmentStatus.Confirmed;
    public bool IsCompleted => Status == AppointmentStatus.Completed;
    public bool IsCancelled => Status == AppointmentStatus.Cancelled;
}
