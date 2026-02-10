using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.Constants;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Appointment for consultation/cash payment
/// </summary>
public class Appointment : AuditableEntity
{
    public string AppointmentNumber { get; set; } = null!;
    public Guid ClinicBranchId { get; set; }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    
    public Guid AppointmentTypeId { get; set; }
    public AppointmentType AppointmentType { get; set; } = null!;
    
    public DateTime AppointmentDate { get; set; }
    public short QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; }

    public decimal FinalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PaidAmount { get; set; }
    
    // Calculated properties - pure business logic
    public decimal RemainingAmount => FinalPrice - DiscountAmount - PaidAmount;
    public bool IsFullyPaid => RemainingAmount <= 0;
    public bool IsPartiallyPaid => PaidAmount > 0 && !IsFullyPaid;
    public bool IsPending => Status == AppointmentStatus.Pending;
    public bool IsConfirmed => Status == AppointmentStatus.Confirmed;
    public bool IsCompleted => Status == AppointmentStatus.Completed;
    public bool IsCancelled => Status == AppointmentStatus.Cancelled;
    
    /// <summary>
    /// Confirms the appointment - state transition logic
    /// </summary>
    public void Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidAppointmentStateException(Status, "confirm", MessageCodes.Domain.INVALID_APPOINTMENT_STATE);
        
        Status = AppointmentStatus.Confirmed;
    }
    
    /// <summary>
    /// Completes the appointment - state transition logic
    /// </summary>
    public void Complete()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidAppointmentStateException(Status, "complete", MessageCodes.Domain.INVALID_APPOINTMENT_STATE);
        
        Status = AppointmentStatus.Completed;
    }
    
    /// <summary>
    /// Cancels the appointment - state transition logic
    /// </summary>
    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed)
            throw new InvalidAppointmentStateException(Status, "cancel", MessageCodes.Domain.APPOINTMENT_ALREADY_COMPLETED);
        
        if (Status == AppointmentStatus.Cancelled)
            return; // Already cancelled
        
        Status = AppointmentStatus.Cancelled;
    }
    
    /// <summary>
    /// Applies a discount - business rule: discount cannot exceed price
    /// </summary>
    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount > FinalPrice)
            throw new InvalidDiscountException(discountAmount, FinalPrice, MessageCodes.Domain.INVALID_DISCOUNT);
        
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidAppointmentStateException(Status, "apply discount", MessageCodes.Domain.INVALID_APPOINTMENT_STATE);
        
        DiscountAmount = discountAmount;
    }
    
    /// <summary>
    /// Records a payment - business rule: payment cannot exceed remaining amount
    /// </summary>
    public void RecordPayment(decimal amount)
    {
        if (amount > RemainingAmount)
            throw new InvalidBusinessOperationException("Payment amount exceeds remaining amount", MessageCodes.Domain.PAYMENT_EXCEEDS_REMAINING);
        
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidAppointmentStateException(Status, "record payment", MessageCodes.Domain.APPOINTMENT_CANCELLED);
        
        PaidAmount += amount;
    }
}
