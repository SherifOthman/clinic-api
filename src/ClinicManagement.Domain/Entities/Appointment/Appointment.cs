using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Events;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Appointment aggregate root - manages appointment lifecycle, payments, and state transitions
/// Enforces business rules and maintains consistency
/// </summary>
public class Appointment : AggregateRoot
{
    // Private constructor for EF Core
    private Appointment() { }

    public string AppointmentNumber { get; private set; } = null!;
    public Guid ClinicBranchId { get; private set; }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    
    public Guid PatientId { get; private set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid DoctorId { get; private set; }
    public Doctor Doctor { get; set; } = null!;
    
    public Guid AppointmentTypeId { get; private set; }
    public AppointmentType AppointmentType { get; set; } = null!;
    
    public DateTime AppointmentDate { get; private set; }
    public short QueueNumber { get; private set; }
    public AppointmentStatus Status { get; private set; }
    
    // Link to invoice (for consultation fee payment)
    public Guid? InvoiceId { get; private set; }
    public Invoice? Invoice { get; set; }
    
    // Calculated properties - pure business logic
    public bool IsConsultationFeePaid => Invoice?.IsFullyPaid ?? false;
    public bool IsPending => Status == AppointmentStatus.Pending;
    public bool IsConfirmed => Status == AppointmentStatus.Confirmed;
    public bool IsCompleted => Status == AppointmentStatus.Completed;
    public bool IsCancelled => Status == AppointmentStatus.Cancelled;

    /// <summary>
    /// Factory method to create a new appointment
    /// Ensures all invariants are met
    /// </summary>
    public static Appointment Create(
        string appointmentNumber,
        Guid clinicBranchId,
        Guid patientId,
        Guid doctorId,
        Guid appointmentTypeId,
        DateTime appointmentDate,
        short queueNumber)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(appointmentNumber))
            throw new InvalidBusinessOperationException("Appointment number is required");
        
        if (clinicBranchId == Guid.Empty)
            throw new InvalidBusinessOperationException("Clinic branch ID is required");
        
        if (patientId == Guid.Empty)
            throw new InvalidBusinessOperationException("Patient ID is required");
        
        if (doctorId == Guid.Empty)
            throw new InvalidBusinessOperationException("Doctor ID is required");
        
        if (appointmentTypeId == Guid.Empty)
            throw new InvalidBusinessOperationException("Appointment type ID is required");
        
        if (appointmentDate < DateTime.UtcNow.Date)
            throw new InvalidBusinessOperationException("Appointment date cannot be in the past");
        
        if (queueNumber <= 0)
            throw new InvalidBusinessOperationException("Queue number must be positive");

        var appointment = new Appointment
        {
            AppointmentNumber = appointmentNumber,
            ClinicBranchId = clinicBranchId,
            PatientId = patientId,
            DoctorId = doctorId,
            AppointmentTypeId = appointmentTypeId,
            AppointmentDate = appointmentDate,
            QueueNumber = queueNumber,
            Status = AppointmentStatus.Pending
        };

        // Raise domain event
        appointment.AddDomainEvent(new AppointmentCreatedEvent(
            appointment.Id,
            appointment.ClinicBranchId,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.AppointmentNumber,
            appointment.AppointmentDate,
            appointment.QueueNumber
        ));

        return appointment;
    }
    
    /// <summary>
    /// Updates appointment date and queue number
    /// </summary>
    public void Reschedule(DateTime newAppointmentDate, short newQueueNumber)
    {
        if (Status == AppointmentStatus.Completed)
            throw new InvalidAppointmentStateException(Status, "reschedule");
        
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidAppointmentStateException(Status, "reschedule");
        
        if (newAppointmentDate < DateTime.UtcNow.Date)
            throw new InvalidBusinessOperationException("Appointment date cannot be in the past");
        
        if (newQueueNumber <= 0)
            throw new InvalidBusinessOperationException("Queue number must be positive");

        AppointmentDate = newAppointmentDate;
        QueueNumber = newQueueNumber;
    }
    
    /// <summary>
    /// Links an invoice to this appointment (for consultation fee)
    /// </summary>
    public void LinkInvoice(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
            throw new InvalidBusinessOperationException("Invoice ID is required");
        
        InvoiceId = invoiceId;
    }
    
    /// <summary>
    /// Confirms the appointment - state transition logic
    /// </summary>
    public void Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidAppointmentStateException(Status, "confirm");
        
        Status = AppointmentStatus.Confirmed;
        
        // Raise domain event
        AddDomainEvent(new AppointmentConfirmedEvent(
            Id,
            PatientId,
            DoctorId,
            AppointmentDate,
            QueueNumber
        ));
    }
    
    /// <summary>
    /// Completes the appointment - state transition logic
    /// </summary>
    public void Complete()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidAppointmentStateException(Status, "complete");
        
        Status = AppointmentStatus.Completed;
        
        // Raise domain event
        AddDomainEvent(new AppointmentCompletedEvent(
            Id,
            PatientId,
            DoctorId,
            AppointmentDate
        ));
    }
    
    /// <summary>
    /// Cancels the appointment - state transition logic
    /// </summary>
    public void Cancel(string reason = "")
    {
        if (Status == AppointmentStatus.Completed)
            throw new InvalidAppointmentStateException(Status, "cancel");
        
        if (Status == AppointmentStatus.Cancelled)
            return; // Already cancelled - idempotent
        
        Status = AppointmentStatus.Cancelled;
        
        // Raise domain event
        AddDomainEvent(new AppointmentCancelledEvent(
            Id,
            PatientId,
            DoctorId,
            AppointmentDate,
            reason
        ));
    }
}
