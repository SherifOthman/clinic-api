using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents ANY billable item in a visit:
/// - Consultation fee
/// - Medical service
/// - Operation
/// - Pharmacy item
/// - Lab test
/// - Radiology
/// ALL money is tracked via VisitServiceItem, NOT directly in Appointment.
/// </summary>
public class VisitServiceItem : AuditableEntity
{
    public Guid VisitId { get; set; }
    public Guid? MedicalServiceId { get; set; } // For services/operations
    public Guid? InventoryItemId { get; set; } // For pharmacy items
    
    // Item details
    public string ItemName { get; set; } = string.Empty; // Captured at transaction time
    public string? ItemDescription { get; set; }
    public ServiceType ServiceType { get; set; } // Consultation, Service, Operation, Pharmacy, Lab, Radiology
    
    // Pricing
    public decimal Price { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public int Quantity { get; set; } = 1; // For pharmacy items
    
    // Navigation properties
    public Visit Visit { get; set; } = null!;
    public MedicalService? MedicalService { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
    // Business Logic (Pure Domain Logic)
    public decimal CalculateTotalAmount()
    {
        return (Price * Quantity) - DiscountAmount;
    }
    
    public decimal CalculatePaidAmount()
    {
        return Payments?.Sum(p => p.Amount) ?? 0;
    }
    
    public decimal CalculateRemainingAmount()
    {
        return CalculateTotalAmount() - CalculatePaidAmount();
    }
    
    public bool IsFullyPaid()
    {
        return CalculateRemainingAmount() <= 0;
    }
    
    public bool SupportsInstallments()
    {
        return CalculateRemainingAmount() > 0;
    }
}