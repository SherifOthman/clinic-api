using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity, INoAuditLog
{
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }

    // ── Old FK — kept during migration ──
    public Guid DoctorId { get; set; }
    public Guid DoctorVisitTypeId { get; set; }

    // ── New FKs — used going forward ──
    /// <summary>Links to DoctorInfo (new model). Nullable during migration.</summary>
    public Guid? DoctorInfoId { get; set; }
    /// <summary>Links to VisitType (new model). Nullable during migration.</summary>
    public Guid? VisitTypeId { get; set; }

    public DateOnly Date { get; set; }
    public int? QueueNumber { get; set; }
    public TimeOnly? ScheduledTime { get; set; }

    public AppointmentType Type { get; set; } = AppointmentType.Queue;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public decimal Price { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal FinalPrice { get; set; }

    public Guid? InvoiceId { get; set; }

    // Navigation — old
    public ClinicBranch Branch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public DoctorVisitType DoctorVisitType { get; set; } = null!;

    // Navigation — new
    public DoctorInfo? DoctorInfo { get; set; }
    public VisitType? NewVisitType { get; set; }
}
