# Aggregate Boundaries Implementation - Step 4 of DDD Improvements

## Summary

Started implementing **Aggregate Boundaries** to define clear ownership and enforce business rules through aggregate roots.

## What We Implemented

### Invoice Aggregate Root (Refactored)

**Before (Anemic Model):**

```csharp
public class Invoice : AuditableEntity
{
    public string InvoiceNumber { get; set; } = null!;
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    // Anyone can modify directly!
    invoice.Items.Add(new InvoiceItem { ... });
    invoice.Status = InvoiceStatus.Paid; // Bypassing business rules!
}
```

**After (Aggregate Root):**

```csharp
public class Invoice : AggregateRoot
{
    // Private collections - can only be modified through methods
    private readonly List<InvoiceItem> _items = [];
    private readonly List<Payment> _payments = [];

    // Private setters - can only be set through methods
    public string InvoiceNumber { get; private set; } = null!;
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }

    // Read-only collections
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    // Factory method
    public static Invoice Create(string invoiceNumber, Guid clinicId, Guid patientId, ...)
    {
        // Validation
        return new Invoice { ... };
    }

    // Behavior methods
    public void AddItem(Guid? medicineId, int quantity, decimal unitPrice, ...)
    {
        // Enforce business rules
        if (Status != InvoiceStatus.Draft)
            throw new InvalidInvoiceStateException(...);

        _items.Add(new InvoiceItem { ... });
        RecalculateTotals();
    }

    public void AddPayment(Guid paymentId, decimal amount, PaymentMethod method, ...)
    {
        // Enforce business rules
        if (amount > RemainingAmount)
            throw new InvalidBusinessOperationException(...);

        _payments.Add(new Payment { ... });

        if (IsFullyPaid)
            Status = InvoiceStatus.FullyPaid;
    }
}
```

### Key Changes

#### 1. **Private Collections**

```csharp
// Before: Public, anyone can modify
public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

// After: Private backing field, read-only public property
private readonly List<InvoiceItem> _items = [];
public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
```

**Why:** Prevents external code from bypassing business rules.

#### 2. **Private Setters**

```csharp
// Before: Public setters
public string InvoiceNumber { get; set; } = null!;
public InvoiceStatus Status { get; set; }

// After: Private setters
public string InvoiceNumber { get; private set; } = null!;
public InvoiceStatus Status { get; private set; }
```

**Why:** Properties can only be changed through behavior methods that enforce rules.

#### 3. **Factory Method**

```csharp
// Before: Public constructor
var invoice = new Invoice
{
    InvoiceNumber = "INV-001",
    ClinicId = clinicId,
    PatientId = patientId
};

// After: Factory method with validation
var invoice = Invoice.Create("INV-001", clinicId, patientId);
```

**Why:** Ensures all invariants are met at creation time.

#### 4. **Behavior Methods**

```csharp
// Before: Direct manipulation
invoice.Items.Add(new InvoiceItem { ... });
invoice.Status = InvoiceStatus.Paid;

// After: Behavior methods
invoice.AddItem(medicineId, quantity, unitPrice);
invoice.AddPayment(paymentId, amount, method);
// Status updated automatically based on payments
```

**Why:** Business logic is encapsulated and enforced.

## Benefits

### 1. **Consistency**

All business rules are enforced in one place:

```csharp
invoice.AddPayment(paymentId, 1000, PaymentMethod.Cash);
// ✅ Automatically checks if amount exceeds remaining
// ✅ Automatically updates status if fully paid
// ✅ Total is always correct
```

### 2. **Encapsulation**

Internal state is protected:

```csharp
// ❌ Can't do this anymore
invoice.Items.Add(new InvoiceItem { ... });
invoice.Status = InvoiceStatus.Paid;

// ✅ Must use methods
invoice.AddItem(...);
invoice.AddPayment(...);
```

### 3. **Testability**

Easy to test business logic:

```csharp
[Fact]
public void AddPayment_ExceedingRemaining_ShouldThrow()
{
    var invoice = Invoice.Create(...);
    invoice.AddItem(..., unitPrice: 100);

    // Try to pay more than total
    Action act = () => invoice.AddPayment(..., amount: 200, ...);

    act.Should().Throw<InvalidBusinessOperationException>();
}
```

### 4. **Clear Ownership**

```csharp
// Who owns invoice items? → Invoice
// Who owns payments? → Invoice
// Who calculates totals? → Invoice
// Who validates state transitions? → Invoice
```

## Aggregate Boundaries Identified

### 1. **Patient Aggregate**

```
Patient (Root)
├── PatientPhone (owned)
├── PatientChronicDisease (owned)
└── MedicalFile (owned)
```

### 2. **Invoice Aggregate** ✅ (Implemented)

```
Invoice (Root)
├── InvoiceItem (owned)
└── Payment (owned)
```

### 3. **Appointment Aggregate**

```
Appointment (Root)
└── MedicalVisit (owned)
    ├── MedicalVisitMeasurement (owned)
    ├── MedicalVisitLabTest (owned)
    ├── MedicalVisitRadiology (owned)
    └── Prescription (owned)
        └── PrescriptionItem (owned)
```

### 4. **Medicine Aggregate** (Simple)

```
Medicine (Root)
```

### 5. **Clinic Aggregate**

```
Clinic (Root)
├── ClinicBranch (owned)
│   ├── ClinicBranchPhoneNumber (owned)
│   └── ClinicBranchAppointmentPrice (owned)
└── ClinicOwner (owned)
```

## Aggregate Design Rules Applied

### Rule 1: Small Aggregates ✅

Invoice aggregate is small and focused:

- Invoice (root)
- InvoiceItem (owned)
- Payment (owned)

### Rule 2: Reference by ID ✅

```csharp
public class Invoice : AggregateRoot
{
    public Guid PatientId { get; private set; } // Reference by ID
    public Guid ClinicId { get; private set; }

    // Navigation properties for queries only
    public Patient Patient { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
}
```

### Rule 3: One Transaction Per Aggregate ✅

```csharp
// Modify one aggregate per transaction
var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
invoice.AddItem(...);
invoice.AddPayment(...);
await _unitOfWork.SaveChangesAsync();
```

### Rule 4: Enforce Invariants ✅

```csharp
public void AddPayment(Guid paymentId, decimal amount, ...)
{
    // Enforce invariants
    if (Status == InvoiceStatus.Cancelled)
        throw new InvalidInvoiceStateException(...);

    if (amount <= 0)
        throw new InvalidBusinessOperationException(...);

    if (amount > RemainingAmount)
        throw new InvalidBusinessOperationException(...);

    // Add payment and update status
    _payments.Add(new Payment { ... });

    if (IsFullyPaid)
        Status = InvoiceStatus.FullyPaid;
}
```

## What Needs to Be Updated

The refactoring breaks existing handlers that directly manipulate Invoice properties. This is **intentional** - it forces us to use the proper aggregate methods.

### Handlers That Need Updating:

1. `CreateInvoiceCommand` - Use `Invoice.Create()` factory method
2. `UpdateInvoiceCommand` - Use behavior methods
3. `AddInvoiceItemCommand` - Use `invoice.AddItem()`
4. `AddPaymentCommand` - Use `invoice.AddPayment()`

### Example Fix:

**Before:**

```csharp
var invoice = new Invoice
{
    InvoiceNumber = invoiceNumber,
    ClinicId = clinicId,
    PatientId = patientId,
    Status = InvoiceStatus.Draft
};

invoice.Items.Add(new InvoiceItem { ... });
```

**After:**

```csharp
var invoice = Invoice.Create(invoiceNumber, clinicId, patientId);
invoice.AddItem(medicineId, quantity, unitPrice);
```

## Next Steps

### Option 1: Complete Invoice Aggregate

1. Fix all Invoice-related handlers
2. Update tests
3. Verify API works

### Option 2: Implement More Aggregates

1. Patient aggregate
2. Appointment aggregate
3. Clinic aggregate

### Option 3: Move to Next DDD Concept

1. Domain Services
2. Specifications Pattern
3. Repository Patterns

## Industry Standards

This implementation follows:

✅ **Domain-Driven Design (Eric Evans)**

- Aggregates are consistency boundaries
- Aggregate roots enforce invariants
- Small aggregates

✅ **Clean Architecture (Robert C. Martin)**

- Encapsulation
- Single Responsibility
- Dependency Rule

✅ **Microsoft eShopOnContainers**

- Private collections with read-only properties
- Factory methods
- Behavior methods

✅ **Vaughn Vernon (Implementing DDD)**

- Small aggregates
- Reference by ID
- One transaction per aggregate

## Conclusion

We've successfully refactored the Invoice entity into a proper aggregate root with:

- ✅ Private collections
- ✅ Private setters
- ✅ Factory method
- ✅ Behavior methods
- ✅ Invariant enforcement
- ✅ Clear boundaries

The next step is to update the handlers to use the new aggregate methods, which will make the codebase more maintainable and enforce business rules consistently.

**Status:** Invoice aggregate implemented, handlers need updating.
