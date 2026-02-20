using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity
{
    public string AppointmentNumber { get; set; } = null!;
    public int ClinicBranchId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int AppointmentTypeId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public short QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; }
    public int? InvoiceId { get; set; }

    public bool IsPending => Status == AppointmentStatus.Pending;
    public bool IsConfirmed => Status == AppointmentStatus.Confirmed;
    public bool IsCompleted => Status == AppointmentStatus.Completed;
    public bool IsCancelled => Status == AppointmentStatus.Cancelled;
}
