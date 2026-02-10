using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.Constants;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents an invoice for medical services, medicines, and supplies
/// </summary>
public class Invoice : AuditableEntity
{
    public string InvoiceNumber { get; set; } = null!; // Human-readable: INV-2024-001
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    
    // Optional link to visit or appointment
    public Guid? MedicalVisitId { get; set; }
    public MedicalVisit? MedicalVisit { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    
    // Invoice status and dates
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime? IssuedDate { get; set; }
    public DateTime? DueDate { get; set; }
    
    // Additional fields
    public string? Notes { get; set; }

    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    // Calculated properties
    public decimal SubtotalAmount => Items.Sum(i => i.LineTotal);
    public decimal FinalAmount => SubtotalAmount - Discount + TaxAmount;
    public decimal TotalPaid => Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
    public decimal RemainingAmount => FinalAmount - TotalPaid;
    public bool IsFullyPaid => RemainingAmount <= 0;
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && !IsFullyPaid && Status != InvoiceStatus.Cancelled;
    public bool IsPartiallyPaid => TotalPaid > 0 && !IsFullyPaid;
    
    /// <summary>
    /// Calculates the discount percentage
    /// </summary>
    public decimal DiscountPercentage => SubtotalAmount > 0 ? Math.Round((Discount / SubtotalAmount) * 100, 2) : 0;
    
    /// <summary>
    /// Gets the number of days overdue
    /// </summary>
    public int DaysOverdue => IsOverdue && DueDate.HasValue ? (DateTime.UtcNow.Date - DueDate.Value.Date).Days : 0;
    
    /// <summary>
    /// Adds an item to the invoice and recalculates totals
    /// </summary>
    /// <param name="item">The invoice item to add</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null</exception>
    /// <exception cref="InvalidInvoiceStateException">Thrown when invoice is not in draft state</exception>
    public void AddItem(InvoiceItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (Status != InvoiceStatus.Draft) 
            throw new InvalidInvoiceStateException(Status, "add items", MessageCodes.Domain.INVALID_INVOICE_STATE);
        
        Items.Add(item);
        RecalculateTotals();
    }
    
    /// <summary>
    /// Removes an item from the invoice and recalculates totals
    /// </summary>
    /// <param name="item">The invoice item to remove</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null</exception>
    /// <exception cref="InvalidInvoiceStateException">Thrown when invoice is not in draft state</exception>
    public void RemoveItem(InvoiceItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (Status != InvoiceStatus.Draft) 
            throw new InvalidInvoiceStateException(Status, "remove items", MessageCodes.Domain.INVALID_INVOICE_STATE);
        
        Items.Remove(item);
        RecalculateTotals();
    }
    
    /// <summary>
    /// Applies a discount to the invoice
    /// </summary>
    /// <param name="discountAmount">The discount amount</param>
    /// <exception cref="InvalidDiscountException">Thrown when discount is invalid</exception>
    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount < 0) 
            throw new InvalidDiscountException(discountAmount, SubtotalAmount, MessageCodes.Domain.INVALID_DISCOUNT);
        if (discountAmount > SubtotalAmount) 
            throw new InvalidDiscountException(discountAmount, SubtotalAmount, MessageCodes.Domain.INVALID_DISCOUNT);
        
        Discount = discountAmount;
        RecalculateTotals();
    }
    
    /// <summary>
    /// Issues the invoice (changes status from Draft to Issued)
    /// </summary>
    /// <exception cref="InvalidInvoiceStateException">Thrown when invoice is not in draft state</exception>
    /// <exception cref="InvalidBusinessOperationException">Thrown when invoice has no items</exception>
    public void Issue()
    {
        if (Status != InvoiceStatus.Draft) 
            throw new InvalidInvoiceStateException(Status, "issue", MessageCodes.Domain.INVALID_INVOICE_STATE);
        if (!Items.Any()) 
            throw new InvalidBusinessOperationException("Cannot issue an invoice without items", MessageCodes.Domain.EMPTY_INVOICE_ITEMS);
        
        Status = InvoiceStatus.Issued;
        IssuedDate = DateTime.UtcNow;
        
        // Set due date if not already set (default to 30 days)
        if (!DueDate.HasValue)
        {
            DueDate = DateTime.UtcNow.AddDays(30);
        }
    }
    
    /// <summary>
    /// Marks the invoice as paid
    /// </summary>
    /// <exception cref="InvalidInvoiceStateException">Thrown when invoice is cancelled</exception>
    public void MarkAsPaid()
    {
        if (Status == InvoiceStatus.Cancelled) 
            throw new InvalidInvoiceStateException(Status, "mark as paid", MessageCodes.Domain.INVOICE_CANCELLED);
        if (IsFullyPaid) return; // Already paid
        
        Status = InvoiceStatus.FullyPaid;
    }
    
    /// <summary>
    /// Cancels the invoice
    /// </summary>
    /// <param name="reason">Reason for cancellation</param>
    /// <exception cref="InvalidInvoiceStateException">Thrown when invoice is already paid</exception>
    public void Cancel(string? reason = null)
    {
        if (Status == InvoiceStatus.FullyPaid) 
            throw new InvalidInvoiceStateException(Status, "cancel", MessageCodes.Domain.INVOICE_ALREADY_PAID);
        
        Status = InvoiceStatus.Cancelled;
        if (!string.IsNullOrEmpty(reason))
        {
            Notes = string.IsNullOrEmpty(Notes) ? $"Cancelled: {reason}" : $"{Notes}\nCancelled: {reason}";
        }
    }
    
    /// <summary>
    /// Recalculates the total amount based on items
    /// </summary>
    private void RecalculateTotals()
    {
        TotalAmount = SubtotalAmount;
    }
    
    /// <summary>
    /// Validates the invoice before saving
    /// </summary>
    /// <exception cref="InvalidBusinessOperationException">Thrown when validation fails</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(InvoiceNumber))
            throw new InvalidBusinessOperationException("Invoice number is required", MessageCodes.Domain.INVOICE_VALIDATION_FAILED);
            
        if (ClinicId == Guid.Empty)
            throw new InvalidBusinessOperationException("Clinic ID is required", MessageCodes.Domain.INVOICE_VALIDATION_FAILED);
            
        if (PatientId == Guid.Empty)
            throw new InvalidBusinessOperationException("Patient ID is required", MessageCodes.Domain.INVOICE_VALIDATION_FAILED);
            
        if (Discount < 0)
            throw new InvalidDiscountException(Discount, SubtotalAmount, MessageCodes.Domain.INVALID_DISCOUNT);
            
        if (Discount > SubtotalAmount)
            throw new InvalidDiscountException(Discount, SubtotalAmount, MessageCodes.Domain.INVALID_DISCOUNT);
    }
}
