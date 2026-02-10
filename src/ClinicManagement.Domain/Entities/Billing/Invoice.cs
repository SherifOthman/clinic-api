using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Events;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Invoice aggregate root - manages invoice items and payments
/// Enforces business rules and maintains consistency
/// </summary>
public class Invoice : AggregateRoot
{
    // Private collections - can only be modified through methods
    private readonly List<InvoiceItem> _items = [];
    private readonly List<Payment> _payments = [];

    // Private constructor for EF Core
    private Invoice() { }

    public string InvoiceNumber { get; private set; } = null!;
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    
    // Optional links (flexible for different scenarios)
    public Guid? AppointmentId { get; private set; }  // Link to appointment (consultation fee)
    public Guid? MedicalVisitId { get; private set; }  // Link to visit (services during visit)
    
    public decimal TotalAmount { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TaxAmount { get; private set; }
    
    // Invoice status and dates
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public DateTime? IssuedDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation properties (for queries only)
    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    public MedicalVisit? MedicalVisit { get; set; }
    
    // Read-only collections
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    // Calculated properties
    public decimal SubtotalAmount => _items.Sum(i => i.LineTotal);
    public decimal FinalAmount => SubtotalAmount - Discount + TaxAmount;
    public decimal TotalPaid => _payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
    public decimal RemainingAmount => FinalAmount - TotalPaid;
    public bool IsFullyPaid => RemainingAmount <= 0;
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && !IsFullyPaid && Status != InvoiceStatus.Cancelled;
    public bool IsPartiallyPaid => TotalPaid > 0 && !IsFullyPaid;
    public decimal DiscountPercentage => SubtotalAmount > 0 ? Math.Round((Discount / SubtotalAmount) * 100, 2) : 0;
    public int DaysOverdue => IsOverdue && DueDate.HasValue ? (DateTime.UtcNow.Date - DueDate.Value.Date).Days : 0;
    
    /// <summary>
    /// Factory method to create a new invoice
    /// </summary>
    public static Invoice Create(
        string invoiceNumber,
        Guid clinicId,
        Guid patientId,
        Guid? appointmentId = null,
        Guid? medicalVisitId = null,
        DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new InvalidBusinessOperationException("Invoice number is required");
        
        if (clinicId == Guid.Empty)
            throw new InvalidBusinessOperationException("Clinic ID is required");
        
        if (patientId == Guid.Empty)
            throw new InvalidBusinessOperationException("Patient ID is required");

        return new Invoice
        {
            InvoiceNumber = invoiceNumber,
            ClinicId = clinicId,
            PatientId = patientId,
            AppointmentId = appointmentId,
            MedicalVisitId = medicalVisitId,
            DueDate = dueDate,
            Status = InvoiceStatus.Draft
        };
    }
    
    /// <summary>
    /// Adds an item to the invoice
    /// </summary>
    public void AddItem(
        Guid? medicalServiceId = null,
        Guid? medicineId = null,
        Guid? medicalSupplyId = null,
        int quantity = 1,
        decimal unitPrice = 0,
        SaleUnit? saleUnit = null)
    {
        if (Status != InvoiceStatus.Draft) 
            throw new InvalidInvoiceStateException(Status, "add items");
        
        // Validate that exactly one item type is specified
        var itemCount = new[] { medicalServiceId, medicineId, medicalSupplyId }.Count(id => id.HasValue);
        if (itemCount != 1)
            throw new InvalidBusinessOperationException(
                "Exactly one of MedicalServiceId, MedicineId, or MedicalSupplyId must be specified");
        
        if (quantity <= 0)
            throw new InvalidBusinessOperationException("Quantity must be positive");
        
        if (unitPrice < 0)
            throw new InvalidBusinessOperationException("Unit price cannot be negative");
        
        var item = new InvoiceItem
        {
            InvoiceId = Id,
            MedicalServiceId = medicalServiceId,
            MedicineId = medicineId,
            MedicalSupplyId = medicalSupplyId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            SaleUnit = saleUnit
        };
        
        _items.Add(item);
        RecalculateTotals();
    }
    
    /// <summary>
    /// Removes an item from the invoice
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        if (Status != InvoiceStatus.Draft) 
            throw new InvalidInvoiceStateException(Status, "remove items");
        
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidBusinessOperationException("Item not found");
        
        _items.Remove(item);
        RecalculateTotals();
    }
    
    /// <summary>
    /// Updates item quantity
    /// </summary>
    public void UpdateItemQuantity(Guid itemId, int newQuantity)
    {
        if (Status != InvoiceStatus.Draft) 
            throw new InvalidInvoiceStateException(Status, "update items");
        
        if (newQuantity <= 0)
            throw new InvalidBusinessOperationException("Quantity must be positive");
        
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidBusinessOperationException("Item not found");
        
        item.Quantity = newQuantity;
        RecalculateTotals();
    }
    
    /// <summary>
    /// Applies a discount to the invoice
    /// </summary>
    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount < 0) 
            throw new InvalidDiscountException(discountAmount, SubtotalAmount);
        
        if (discountAmount > SubtotalAmount) 
            throw new InvalidDiscountException(discountAmount, SubtotalAmount);
        
        Discount = discountAmount;
        RecalculateTotals();
    }
    
    /// <summary>
    /// Applies a percentage discount
    /// </summary>
    public void ApplyPercentageDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new InvalidDiscountException(percentage, 100);
        
        var discountAmount = SubtotalAmount * (percentage / 100);
        ApplyDiscount(discountAmount);
    }
    
    /// <summary>
    /// Sets tax amount
    /// </summary>
    public void SetTax(decimal taxAmount)
    {
        if (taxAmount < 0)
            throw new InvalidBusinessOperationException("Tax amount cannot be negative");
        
        TaxAmount = taxAmount;
        RecalculateTotals();
    }
    
    /// <summary>
    /// Issues the invoice (changes status from Draft to Issued)
    /// </summary>
    public void Issue()
    {
        if (Status != InvoiceStatus.Draft) 
            throw new InvalidInvoiceStateException(Status, "issue");
        
        if (!_items.Any()) 
            throw new InvalidBusinessOperationException("Cannot issue an invoice without items");
        
        Status = InvoiceStatus.Issued;
        IssuedDate = DateTime.UtcNow;
        
        // Set due date if not already set (default to 30 days)
        DueDate ??= DateTime.UtcNow.AddDays(30);
        
        // Raise domain event
        AddDomainEvent(new InvoiceIssuedEvent(
            Id,
            InvoiceNumber,
            PatientId,
            ClinicId,
            SubtotalAmount,
            Discount,
            TaxAmount,
            FinalAmount,
            IssuedDate.Value
        ));
    }
    
    /// <summary>
    /// Adds a payment to the invoice
    /// </summary>
    public void AddPayment(Guid paymentId, decimal amount, PaymentMethod method, string? referenceNumber = null)
    {
        if (Status == InvoiceStatus.Cancelled) 
            throw new InvalidInvoiceStateException(Status, "add payment");
        
        if (amount <= 0)
            throw new InvalidBusinessOperationException("Payment amount must be positive");
        
        if (amount > RemainingAmount)
            throw new InvalidBusinessOperationException(
                $"Payment amount ({amount}) exceeds remaining amount ({RemainingAmount})");
        
        var payment = new Payment
        {
            Id = paymentId,
            InvoiceId = Id,
            Amount = amount,
            PaymentMethod = method,
            ReferenceNumber = referenceNumber,
            PaymentDate = DateTime.UtcNow,
            Status = PaymentStatus.Paid
        };
        
        _payments.Add(payment);
        
        var wasFullyPaid = IsFullyPaid;
        
        // Update invoice status if fully paid
        if (IsFullyPaid)
        {
            Status = InvoiceStatus.FullyPaid;
        }
        else if (TotalPaid > 0)
        {
            Status = InvoiceStatus.PartiallyPaid;
        }
        
        // Raise payment recorded event
        AddDomainEvent(new InvoicePaymentRecordedEvent(
            Id,
            InvoiceNumber,
            PatientId,
            amount,
            method,
            RemainingAmount,
            IsFullyPaid
        ));
        
        // Raise fully paid event if this payment completed the invoice
        if (IsFullyPaid && !wasFullyPaid)
        {
            AddDomainEvent(new InvoiceFullyPaidEvent(
                Id,
                InvoiceNumber,
                PatientId,
                FinalAmount,
                TotalPaid,
                DateTime.UtcNow
            ));
        }
    }
    
    /// <summary>
    /// Cancels the invoice
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status == InvoiceStatus.FullyPaid) 
            throw new InvalidInvoiceStateException(Status, "cancel");
        
        Status = InvoiceStatus.Cancelled;
        
        if (!string.IsNullOrEmpty(reason))
        {
            Notes = string.IsNullOrEmpty(Notes) ? $"Cancelled: {reason}" : $"{Notes}\nCancelled: {reason}";
        }
        
        // Raise domain event
        AddDomainEvent(new InvoiceCancelledEvent(
            Id,
            InvoiceNumber,
            PatientId,
            FinalAmount,
            reason ?? "No reason provided"
        ));
    }
    
    /// <summary>
    /// Updates notes
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }
    
    /// <summary>
    /// Recalculates the total amount based on items
    /// </summary>
    private void RecalculateTotals()
    {
        TotalAmount = SubtotalAmount - Discount + TaxAmount;
    }
}