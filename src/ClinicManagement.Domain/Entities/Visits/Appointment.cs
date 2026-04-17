using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity, INoAuditLog
{
    public Guid BranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorInfoId { get; set; }
    public Guid VisitTypeId { get; set; }

    public DateOnly Date { get; set; }
    public int? QueueNumber { get; set; }
    public TimeOnly? ScheduledTime { get; set; }

    public AppointmentType Type { get; set; } = AppointmentType.Queue;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public decimal Price { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal FinalPrice { get; set; }

    public Guid? InvoiceId { get; set; }

    // Navigation
    public ClinicBranch Branch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public DoctorInfo Doctor { get; set; } = null!;
    public VisitType VisitType { get; set; } = null!;
}
