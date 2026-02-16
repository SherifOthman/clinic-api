using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Entities;

public class Appointment : AuditableEntity
{
    public string AppointmentNumber { get; set; } = null!;
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid AppointmentTypeId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public short QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; }
    public Guid? InvoiceId { get; set; }

    // Navigation properties
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public DoctorProfile DoctorProfile { get; set; } = null!;
    public AppointmentType AppointmentType { get; set; } = null!;
    public Invoice? Invoice { get; set; }
    
    // Calculated properties
    public bool IsConsultationFeePaid => Invoice?.IsFullyPaid ?? false;
    public bool IsPending => Status == AppointmentStatus.Pending;
    public bool IsConfirmed => Status == AppointmentStatus.Confirmed;
    public bool IsCompleted => Status == AppointmentStatus.Completed;
    public bool IsCancelled => Status == AppointmentStatus.Cancelled;
}
