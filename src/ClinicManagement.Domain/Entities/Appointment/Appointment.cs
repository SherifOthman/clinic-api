using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Appointment for consultation/cash payment
/// </summary>
public class Appointment : AuditableEntity
{
    public string AppointmentNumber { get; set; } = null!; // Human-readable: APT-2024-001
    public Guid ClinicBranchId { get; set; }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid DoctorId { get; set; }
    public Staff Doctor { get; set; } = null!;
    
    public Guid AppointmentTypeId { get; set; }
    public AppointmentType AppointmentType { get; set; } = null!;
    
    public DateTime AppointmentDate { get; set; }
    public short QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; }

    public decimal FinalPrice { get; set; }
     
    public decimal DiscountAmount { get; set; }
    
    public decimal PaidAmount { get; set; }
    
    public decimal RemainingAmount => FinalPrice - DiscountAmount - PaidAmount;
}
