using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Events;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Medicine dispensing aggregate root - tracks actual medicine given to patients
/// Supports dispensing with or without visit (e.g., chronic disease refills)
/// </summary>
public class MedicineDispensing : AggregateRoot
{
    // Private constructor for EF Core
    private MedicineDispensing() { }

    public Guid ClinicBranchId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid MedicineId { get; private set; }
    
    // Optional links (flexible for different scenarios)
    public Guid? VisitId { get; private set; }
    public Guid? PrescriptionId { get; private set; }
    public Guid? PrescriptionItemId { get; private set; }
    
    // Dispensing details
    public int Quantity { get; private set; }
    public SaleUnit Unit { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    
    // Tracking
    public Guid DispensedByUserId { get; private set; }
    public DateTime DispensedAt { get; private set; }
    public DispensingStatus Status { get; private set; }
    public string? Notes { get; private set; }
    
    // Navigation properties
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
    public MedicalVisit? Visit { get; set; }
    public Prescription? Prescription { get; set; }
    public User DispensedBy { get; set; } = null!;
    
    /// <summary>
    /// Factory method to create a new medicine dispensing record
    /// </summary>
    public static MedicineDispensing Create(
        Guid clinicBranchId,
        Guid patientId,
        Guid medicineId,
        int quantity,
        SaleUnit unit,
        decimal unitPrice,
        Guid dispensedByUserId,
        Guid? visitId = null,
        Guid? prescriptionId = null,
        Guid? prescriptionItemId = null,
        string? notes = null)
    {
        // Validation
        if (clinicBranchId == Guid.Empty)
            throw new InvalidBusinessOperationException("Clinic branch ID is required");
        
        if (patientId == Guid.Empty)
            throw new InvalidBusinessOperationException("Patient ID is required");
        
        if (medicineId == Guid.Empty)
            throw new InvalidBusinessOperationException("Medicine ID is required");
        
        if (quantity <= 0)
            throw new InvalidBusinessOperationException("Quantity must be positive");
        
        if (unitPrice < 0)
            throw new InvalidBusinessOperationException("Unit price cannot be negative");
        
        if (dispensedByUserId == Guid.Empty)
            throw new InvalidBusinessOperationException("Dispensed by user ID is required");
        
        var dispensing = new MedicineDispensing
        {
            ClinicBranchId = clinicBranchId,
            PatientId = patientId,
            MedicineId = medicineId,
            VisitId = visitId,
            PrescriptionId = prescriptionId,
            PrescriptionItemId = prescriptionItemId,
            Quantity = quantity,
            Unit = unit,
            UnitPrice = unitPrice,
            TotalPrice = quantity * unitPrice,
            DispensedByUserId = dispensedByUserId,
            DispensedAt = DateTime.UtcNow,
            Status = DispensingStatus.Dispensed,
            Notes = notes
        };
        
        // Raise domain event
        dispensing.AddDomainEvent(new MedicineDispensedEvent(
            dispensing.Id,
            medicineId,
            patientId,
            quantity,
            unit,
            visitId
        ));
        
        return dispensing;
    }
    
    /// <summary>
    /// Cancels the dispensing (e.g., patient returned medicine)
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == DispensingStatus.Cancelled)
            return; // Already cancelled - idempotent
        
        Status = DispensingStatus.Cancelled;
        Notes = reason;
        
        AddDomainEvent(new MedicineDispensingCancelledEvent(
            Id,
            MedicineId,
            PatientId,
            Quantity,
            Unit,
            reason
        ));
    }
    
    /// <summary>
    /// Marks as partially dispensed (some items out of stock)
    /// </summary>
    public void MarkAsPartiallyDispensed(int actualQuantity, string reason)
    {
        if (actualQuantity <= 0)
            throw new InvalidBusinessOperationException("Actual quantity must be positive");
        
        if (actualQuantity >= Quantity)
            throw new InvalidBusinessOperationException("Actual quantity must be less than requested quantity");
        
        Status = DispensingStatus.PartiallyDispensed;
        Quantity = actualQuantity;
        TotalPrice = actualQuantity * UnitPrice;
        Notes = reason;
    }
}

/// <summary>
/// Status of medicine dispensing
/// </summary>
public enum DispensingStatus
{
    Pending,              // Prescription received, not yet dispensed
    Dispensed,            // Medicine given to patient
    PartiallyDispensed,   // Some items given, some out of stock
    Cancelled             // Patient didn't take medicine or returned it
}
