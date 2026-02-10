# Aggregate Boundaries - Step 4 of DDD Improvements

## What Are Aggregates?

**Aggregates** are clusters of domain objects (entities and value objects) that are treated as a single unit for data changes. Each aggregate has a root entity called the **Aggregate Root**.

### Key Concepts:

1. **Aggregate Root** - The only entity that external objects can hold references to
2. **Boundary** - Defines what's inside vs outside the aggregate
3. **Consistency Boundary** - All rules within an aggregate are always consistent
4. **Transaction Boundary** - Changes to an aggregate happen in one transaction

## Why Aggregate Boundaries Matter

### Problem: Without Clear Boundaries

```csharp
// ❌ BAD: Direct access to nested entities
var patient = await _context.Patients.FindAsync(id);
var phone = patient.PhoneNumbers.First();
phone.PhoneNumber = "+1234567890"; // Bypassing Patient logic!
await _context.SaveChangesAsync();

// ❌ BAD: Modifying multiple aggregates in complex ways
var invoice = await _context.Invoices.FindAsync(invoiceId);
var patient = await _context.Patients.FindAsync(patientId);
invoice.PatientId = patient.Id;
patient.Invoices.Add(invoice);
invoice.Items.Add(new InvoiceItem { ... });
// Who's responsible? What are the rules?
```

**Problems:**

- ❌ Unclear ownership
- ❌ Invariants can be violated
- ❌ Concurrency issues
- ❌ Hard to maintain
- ❌ Business rules scattered

### Solution: Clear Aggregate Boundaries

```csharp
// ✅ GOOD: All changes through aggregate root
var patient = await _unitOfWork.Patients.GetByIdAsync(id);
patient.UpdatePhoneNumber(phoneId, "+1234567890"); // Patient enforces rules
await _unitOfWork.SaveChangesAsync();

// ✅ GOOD: Each aggregate is independent
var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
invoice.AddItem(medicine, quantity, price); // Invoice enforces rules
await _unitOfWork.SaveChangesAsync();
```

## Identifying Aggregates in Your System

### Current Entities Analysis

Let's analyze your domain to identify aggregates:

#### 1. **Patient Aggregate**

```
Patient (Root)
├── PatientPhone (owned)
├── PatientChronicDisease (owned)
└── MedicalFile (owned)
```

**Why Patient is an Aggregate Root:**

- ✅ Has identity (PatientId)
- ✅ Has lifecycle (created, updated, deleted)
- ✅ Owns phone numbers (can't exist without patient)
- ✅ Owns chronic disease associations
- ✅ Owns medical files
- ✅ Enforces invariants (e.g., at least one phone number)

**Boundary Rules:**

- Phone numbers can only be added/removed through Patient
- Chronic diseases can only be associated through Patient
- Medical files belong to Patient

#### 2. **Invoice Aggregate**

```
Invoice (Root)
├── InvoiceItem (owned)
└── Payment (owned)
```

**Why Invoice is an Aggregate Root:**

- ✅ Has identity (InvoiceId)
- ✅ Has lifecycle (draft → issued → paid → cancelled)
- ✅ Owns invoice items (can't exist without invoice)
- ✅ Owns payments
- ✅ Enforces invariants (total = sum of items, payments ≤ total)

**Boundary Rules:**

- Items can only be added/removed through Invoice
- Payments can only be added through Invoice
- Invoice calculates totals and validates payments

#### 3. **Appointment Aggregate**

```
Appointment (Root)
├── MedicalVisit (owned)
│   ├── MedicalVisitMeasurement (owned)
│   ├── MedicalVisitLabTest (owned)
│   ├── MedicalVisitRadiology (owned)
│   └── Prescription (owned)
│       └── PrescriptionItem (owned)
```

**Why Appointment is an Aggregate Root:**

- ✅ Has identity (AppointmentId)
- ✅ Has lifecycle (scheduled → in-progress → completed → cancelled)
- ✅ Owns medical visit (one-to-one)
- ✅ Enforces invariants (can't complete without visit)

**Boundary Rules:**

- Medical visit can only be created through Appointment
- Measurements, tests, prescriptions belong to visit
- Appointment controls state transitions

#### 4. **Medicine Aggregate** (Simple)

```
Medicine (Root)
```

**Why Medicine is an Aggregate Root:**

- ✅ Has identity (MedicineId)
- ✅ Has lifecycle (active → discontinued)
- ✅ Manages its own stock
- ✅ Enforces invariants (stock ≥ 0, price > 0)

**Boundary Rules:**

- Stock can only be modified through Medicine methods
- Price changes through Medicine
- No owned entities (simple aggregate)

#### 5. **Clinic Aggregate**

```
Clinic (Root)
├── ClinicBranch (owned)
│   ├── ClinicBranchPhoneNumber (owned)
│   └── ClinicBranchAppointmentPrice (owned)
└── ClinicOwner (owned)
```

**Why Clinic is an Aggregate Root:**

- ✅ Has identity (ClinicId)
- ✅ Has lifecycle
- ✅ Owns branches
- ✅ Enforces invariants (at least one branch, one owner)

**Boundary Rules:**

- Branches can only be added/removed through Clinic
- Branch phone numbers through ClinicBranch
- Appointment prices through ClinicBranch

### Reference Between Aggregates

Aggregates should reference each other by **ID only**, not by navigation properties (or use them carefully):

```csharp
// ✅ GOOD: Reference by ID
public class Invoice : AggregateRoot
{
    public Guid PatientId { get; private set; } // Reference by ID
    public Guid ClinicId { get; private set; }

    // Navigation properties for queries only (not for modifications)
    public Patient Patient { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
}

// ❌ BAD: Modifying through navigation
invoice.Patient.UpdateInfo(...); // Don't do this!

// ✅ GOOD: Load and modify separately
var patient = await _unitOfWork.Patients.GetByIdAsync(invoice.PatientId);
patient.UpdateInfo(...);
```

## Aggregate Design Rules

### Rule 1: Small Aggregates

**Keep aggregates as small as possible**

```csharp
// ❌ BAD: Too large
Clinic
├── ClinicBranch
│   ├── Doctor
│   │   ├── Appointment
│   │   │   └── MedicalVisit
│   │   │       └── Prescription
// Too many levels! Concurrency nightmare!

// ✅ GOOD: Separate aggregates
Clinic (aggregate)
├── ClinicBranch

Doctor (aggregate)

Appointment (aggregate)
├── MedicalVisit
    └── Prescription
```

### Rule 2: Reference by ID

**Aggregates reference each other by ID**

```csharp
// ✅ GOOD
public class Appointment : AggregateRoot
{
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }

    // For queries only
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
}
```

### Rule 3: One Transaction Per Aggregate

**Modify one aggregate per transaction**

```csharp
// ❌ BAD: Modifying multiple aggregates
var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
patient.UpdateInfo(...);
invoice.AddItem(...);
await _unitOfWork.SaveChangesAsync(); // Two aggregates in one transaction!

// ✅ GOOD: One aggregate per transaction
var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
invoice.AddItem(...);
await _unitOfWork.SaveChangesAsync();

// If you need to update patient, do it separately
var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
patient.UpdateInfo(...);
await _unitOfWork.SaveChangesAsync();
```

### Rule 4: Enforce Invariants

**Aggregate root enforces all invariants**

```csharp
public class Invoice : AggregateRoot
{
    private readonly List<InvoiceItem> _items = [];
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    public void AddItem(Guid itemId, string name, int quantity, Money price)
    {
        // Enforce invariants
        if (Status == InvoiceStatus.Paid)
            throw new InvalidInvoiceStateException("Cannot add items to paid invoice");

        if (quantity <= 0)
            throw new InvalidBusinessOperationException("Quantity must be positive");

        if (price.Amount <= 0)
            throw new InvalidBusinessOperationException("Price must be positive");

        _items.Add(new InvoiceItem
        {
            Id = itemId,
            Name = name,
            Quantity = quantity,
            UnitPrice = price
        });

        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        Total = _items.Sum(i => i.Quantity * i.UnitPrice.Amount);
    }
}
```

## Aggregate Patterns in Your System

### Pattern 1: Simple Aggregate (Medicine)

```csharp
public class Medicine : AggregateRoot
{
    // No owned entities
    // Self-contained
    // Simple invariants

    public void AddStock(int strips)
    {
        if (strips <= 0) throw new InvalidBusinessOperationException(...);
        TotalStripsInStock += strips;
    }
}
```

### Pattern 2: Aggregate with Owned Entities (Patient)

```csharp
public class Patient : AggregateRoot
{
    private readonly List<PatientPhone> _phoneNumbers = [];
    public IReadOnlyCollection<PatientPhone> PhoneNumbers => _phoneNumbers.AsReadOnly();

    public void AddPhoneNumber(string number, bool isPrimary)
    {
        // Enforce invariants
        if (_phoneNumbers.Any(p => p.PhoneNumber == number))
            throw new DuplicatePhoneNumberException();

        if (isPrimary)
        {
            // Only one primary
            foreach (var phone in _phoneNumbers)
                phone.IsPrimary = false;
        }

        _phoneNumbers.Add(new PatientPhone
        {
            PhoneNumber = number,
            IsPrimary = isPrimary
        });
    }
}
```

### Pattern 3: Aggregate with Complex Hierarchy (Appointment)

```csharp
public class Appointment : AggregateRoot
{
    public MedicalVisit? Visit { get; private set; }

    public void StartVisit()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidAppointmentStateException();

        Visit = new MedicalVisit
        {
            StartTime = DateTime.UtcNow
        };

        Status = AppointmentStatus.InProgress;
    }

    public void AddMeasurement(Guid attributeId, string value)
    {
        if (Visit == null)
            throw new InvalidAppointmentStateException("No active visit");

        Visit.Measurements.Add(new MedicalVisitMeasurement
        {
            MeasurementAttributeId = attributeId,
            Value = value
        });
    }
}
```

## Benefits of Clear Aggregate Boundaries

### 1. **Consistency**

All invariants are enforced in one place:

```csharp
// Invoice always consistent
invoice.AddItem(...); // Recalculates total
invoice.AddPayment(...); // Validates against total
// Total is always correct!
```

### 2. **Concurrency**

Smaller aggregates = less locking:

```csharp
// ✅ GOOD: Two users can modify different invoices simultaneously
User1: invoice1.AddItem(...);
User2: invoice2.AddItem(...);
// No conflict!

// ❌ BAD: If Invoice owned Patient, conflict!
User1: invoice1.Patient.UpdateInfo(...);
User2: invoice2.Patient.UpdateInfo(...);
// Conflict! Same patient!
```

### 3. **Testability**

Easy to test aggregate in isolation:

```csharp
[Fact]
public void AddItem_ShouldRecalculateTotal()
{
    var invoice = Invoice.Create(...);
    invoice.AddItem(..., new Money(100, "USD"));
    invoice.AddItem(..., new Money(50, "USD"));

    invoice.Total.Should().Be(new Money(150, "USD"));
}
```

### 4. **Maintainability**

Clear ownership and responsibilities:

```csharp
// Who manages phone numbers? → Patient
// Who manages invoice items? → Invoice
// Who manages stock? → Medicine
```

## Implementation Strategy

We'll implement aggregate boundaries by:

1. **Identify Aggregate Roots** - Already done above
2. **Make Collections Private** - Prevent direct modification
3. **Add Behavior Methods** - All changes through methods
4. **Enforce Invariants** - Validate in methods
5. **Update Handlers** - Use behavior methods instead of direct property access

Let's start implementing!
