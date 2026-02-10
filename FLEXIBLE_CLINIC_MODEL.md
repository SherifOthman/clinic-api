# Flexible Clinic Model - Supporting All Clinic Types

## 🎯 Key Insight: Clinics Operate Differently!

### The Reality:

1. **Some clinics**: Consultation only (no pharmacy, no lab)
2. **Some clinics**: Consultation + pharmacy (no lab)
3. **Some clinics**: Full service (rare)
4. **Medicine refills**: No visit needed (chronic patients)
5. **Lab/Radiology**: Usually external, results brought back later

---

## 🔄 Revised Understanding

### 1. Prescription = Doctor's Instructions (Always Text!)

**Why**: Doctor prescribes generic drug names, patient can buy anywhere

```csharp
public class Prescription : AggregateRoot {
    public Guid VisitId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime PrescribedAt { get; set; }

    public ICollection<PrescriptionItem> Items { get; set; }
}

public class PrescriptionItem : BaseEntity {
    public Guid PrescriptionId { get; set; }

    // Generic drug information (text only)
    public string DrugName { get; set; }  // "Paracetamol"
    public string? Dosage { get; set; }  // "500mg"
    public int FrequencyPerDay { get; set; }  // 3 times
    public int DurationInDays { get; set; }  // 7 days
    public string? Instructions { get; set; }  // "After meals"

    // NO link to Medicine entity!
    // Patient can buy from any pharmacy
}
```

**This is CORRECT for your use case!**

---

### 2. Medicine Dispensing = Optional Feature (If Clinic Has Pharmacy)

**Only create this if clinic dispenses medicine internally:**

```csharp
public class MedicineDispensing : AggregateRoot {
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }

    // Optional links (flexible!)
    public Guid? VisitId { get; set; }  // Null if no visit (refill only)
    public Guid? PrescriptionId { get; set; }  // Null if no prescription (OTC)
    public Guid? PrescriptionItemId { get; set; }  // Which item from prescription

    // Actual medicine from inventory
    public Guid MedicineId { get; set; }
    public int Quantity { get; set; }
    public SaleUnit Unit { get; set; }  // Box or Strip

    // Tracking
    public Guid DispensedByUserId { get; set; }  // Pharmacist
    public DateTime DispensedAt { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Status
    public DispensingStatus Status { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Medicine Medicine { get; set; }
    public Patient Patient { get; set; }
    public MedicalVisit? Visit { get; set; }
    public Prescription? Prescription { get; set; }
}

public enum DispensingStatus {
    Pending,      // Prescription received, not yet dispensed
    Dispensed,    // Medicine given to patient
    PartiallyDispensed,  // Some items given, some out of stock
    Cancelled     // Patient didn't take medicine
}
```

**Use Cases:**

1. **With prescription**: Doctor prescribes → Pharmacy dispenses
2. **Without prescription (OTC)**: Patient buys directly from pharmacy
3. **Refill without visit**: Chronic patient gets regular medicine
4. **Partial dispensing**: Prescription says 30 tablets, pharmacy has 20

---

### 3. Lab Tests = Mostly External, Results Uploaded

**Your current model is actually GOOD for this!**

```csharp
public class MedicalVisitLabTest : BaseEntity {
    public Guid MedicalVisitId { get; set; }
    public Guid LabTestId { get; set; }

    // Status tracking
    public LabTestStatus Status { get; set; }
    public DateTime OrderedAt { get; set; }

    // Results (uploaded by patient or staff)
    public DateTime? ResultsUploadedAt { get; set; }
    public string? ResultsFilePath { get; set; }  // PDF from external lab
    public string? ResultsNotes { get; set; }

    // Review
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByDoctorId { get; set; }
    public string? DoctorNotes { get; set; }

    // Optional: If clinic does test internally
    public Guid? PerformedByUserId { get; set; }
    public DateTime? PerformedAt { get; set; }
}

public enum LabTestStatus {
    Ordered,           // Doctor ordered, patient needs to do it
    ResultsUploaded,   // Patient uploaded results from external lab
    Reviewed,          // Doctor reviewed results
    Cancelled          // Test cancelled
}
```

**Workflow:**

1. Doctor orders test → Status = Ordered
2. Patient goes to external lab → Gets results (PDF)
3. Patient/staff uploads PDF → Status = ResultsUploaded
4. Doctor reviews → Status = Reviewed

**OR (if clinic has lab):**

1. Doctor orders test → Status = Ordered
2. Lab tech performs test → Status = ResultsUploaded
3. Doctor reviews → Status = Reviewed

---

### 4. Invoice = Flexible Billing for Any Service

**Your current model is GOOD! Just clarify the logic:**

```csharp
public class Invoice : AggregateRoot {
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }

    // Optional links (flexible!)
    public Guid? MedicalVisitId { get; set; }  // Null if no visit (pharmacy only)
    public Guid? AppointmentId { get; set; }  // Null if walk-in

    public ICollection<InvoiceItem> Items { get; set; }
    public ICollection<Payment> Payments { get; set; }
}

public class InvoiceItem : BaseEntity {
    public Guid InvoiceId { get; set; }

    // Flexible item types (only ONE should be set)
    public Guid? MedicalServiceId { get; set; }  // Consultation, X-ray, etc.
    public Guid? MedicineDispensingId { get; set; }  // If medicine dispensed from clinic
    public Guid? MedicalSupplyId { get; set; }  // Bandages, etc.

    // Or generic item (for external services)
    public string? ItemDescription { get; set; }  // "External lab test - CBC"

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public SaleUnit? SaleUnit { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}
```

**Use Cases:**

1. **Consultation only**: Invoice with MedicalServiceId (consultation)
2. **Consultation + Medicine**: Invoice with service + dispensing items
3. **Medicine only (refill)**: Invoice with only dispensing items, no visit
4. **External lab fee**: Invoice with ItemDescription (generic)

---

### 5. Appointment = Optional (Walk-ins Allowed!)

**Your fix is correct:**

```csharp
public class Appointment : AggregateRoot {
    // ... existing fields

    public Guid? MedicalVisitId { get; set; }  // Created when doctor starts
    public MedicalVisit? MedicalVisit { get; set; }
}

public class MedicalVisit : AggregateRoot {
    public Guid? AppointmentId { get; set; }  // Null for walk-ins
    public Appointment? Appointment { get; set; }
}
```

**Workflows:**

1. **With appointment**: Appointment → Visit created when doctor starts
2. **Walk-in**: Visit created directly, no appointment
3. **No-show**: Appointment exists, no visit created

---

### 6. Medicine as Service (Pharmacy Without Visit)

**This is the KEY insight!**

```csharp
// Scenario: Patient comes for medicine refill (no doctor visit)

// 1. Create dispensing record
var dispensing = MedicineDispensing.Create(
    clinicBranchId: branchId,
    patientId: patientId,
    medicineId: medicineId,
    quantity: 30,
    unit: SaleUnit.Strip,
    dispensedByUserId: pharmacistId,
    visitId: null,  // No visit!
    prescriptionId: oldPrescriptionId  // Reference to old prescription
);

// 2. Create invoice
var invoice = Invoice.Create(
    clinicId: clinicId,
    patientId: patientId,
    medicalVisitId: null  // No visit!
);

invoice.AddItem(
    medicineDispensingId: dispensing.Id,
    quantity: 30,
    unitPrice: medicine.StripPrice
);

// 3. Patient pays and leaves
invoice.RecordPayment(totalAmount);
```

---

## 🎯 Revised Entity Relationships

### Core Entities (Always Present)

```
Clinic
  ↓
ClinicBranch
  ↓
├─ Doctors (work at branch)
├─ Patients (registered at clinic)
└─ Users (staff)
```

### Optional Features (Depends on Clinic Type)

#### Feature 1: Appointments (Optional)

```
Patient → Appointment → MedicalVisit (maybe)
```

#### Feature 2: Pharmacy (Optional)

```
Medicine (inventory)
  ↓
MedicineDispensing (when sold)
  ↓
InvoiceItem (billing)
```

#### Feature 3: Lab/Radiology (Usually External)

```
Doctor orders → MedicalVisitLabTest (status: Ordered)
  ↓
Patient does externally → Uploads results
  ↓
Doctor reviews → Status: Reviewed
```

#### Feature 4: Medical Services (Always)

```
MedicalService (consultation, X-ray, etc.)
  ↓
InvoiceItem (billing)
```

---

## 🔧 Required Changes

### 1. Make Relationships Nullable (Flexibility)

```csharp
// ✅ Already correct
public class MedicalVisit {
    public Guid? AppointmentId { get; set; }  // Walk-ins
}

// ✅ Already correct
public class Invoice {
    public Guid? MedicalVisitId { get; set; }  // Pharmacy-only sales
}

// ✅ Already correct
public class Prescription {
    public Guid VisitId { get; set; }  // Always from visit
}

// ❌ Need to add
public class MedicineDispensing {
    public Guid? VisitId { get; set; }  // Null for refills
    public Guid? PrescriptionId { get; set; }  // Null for OTC
}
```

### 2. Add MedicineDispensing Entity

```csharp
public class MedicineDispensing : AggregateRoot
{
    private MedicineDispensing() { }

    public Guid ClinicBranchId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid MedicineId { get; private set; }

    // Optional links
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

    // Navigation
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
    public MedicalVisit? Visit { get; set; }
    public Prescription? Prescription { get; set; }
    public User DispensedBy { get; set; } = null!;

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

        if (quantity <= 0)
            throw new InvalidBusinessOperationException("Quantity must be positive");

        if (unitPrice < 0)
            throw new InvalidBusinessOperationException("Unit price cannot be negative");

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

    public void Cancel(string reason)
    {
        if (Status == DispensingStatus.Cancelled)
            return;

        Status = DispensingStatus.Cancelled;
        Notes = reason;

        AddDomainEvent(new MedicineDispensingCancelledEvent(
            Id,
            MedicineId,
            Quantity,
            reason
        ));
    }
}

public enum DispensingStatus
{
    Pending,
    Dispensed,
    PartiallyDispensed,
    Cancelled
}
```

### 3. Update InvoiceItem to Link Dispensing

```csharp
public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }

    // Flexible item types (only ONE should be set)
    public Guid? MedicalServiceId { get; set; }
    public Guid? MedicineDispensingId { get; set; }  // ✨ NEW
    public Guid? MedicalSupplyId { get; set; }

    // For external services (no internal entity)
    public string? ItemDescription { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public SaleUnit? SaleUnit { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;

    // Navigation
    public Invoice Invoice { get; set; } = null!;
    public MedicalService? MedicalService { get; set; }
    public MedicineDispensing? MedicineDispensing { get; set; }  // ✨ NEW
    public MedicalSupply? MedicalSupply { get; set; }
}
```

### 4. Update Lab Test Status

```csharp
public class MedicalVisitLabTest : BaseEntity
{
    public Guid MedicalVisitId { get; set; }
    public Guid LabTestId { get; set; }

    // Workflow
    public LabTestStatus Status { get; set; }
    public DateTime OrderedAt { get; set; }
    public Guid OrderedByDoctorId { get; set; }

    // Results (from external or internal lab)
    public DateTime? ResultsUploadedAt { get; set; }
    public Guid? ResultsUploadedByUserId { get; set; }
    public string? ResultsFilePath { get; set; }
    public string? ResultsText { get; set; }

    // Review
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByDoctorId { get; set; }
    public string? DoctorNotes { get; set; }

    // Optional: If done internally
    public Guid? PerformedByUserId { get; set; }
    public DateTime? PerformedAt { get; set; }

    public string? Notes { get; set; }

    // Navigation
    public MedicalVisit MedicalVisit { get; set; } = null!;
    public LabTest LabTest { get; set; } = null!;
}

public enum LabTestStatus
{
    Ordered,           // Doctor ordered
    ResultsUploaded,   // Results available (external or internal)
    Reviewed,          // Doctor reviewed
    Cancelled          // Cancelled
}
```

### 5. Add Patient Safety Fields

```csharp
public class Patient : AggregateRoot
{
    // ... existing fields

    // Critical medical information
    public string? BloodType { get; set; }
    public string? KnownAllergies { get; set; }  // Text field for quick reference

    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }

    // Collections
    public ICollection<PatientAllergy> Allergies { get; set; } = new List<PatientAllergy>();
}

public class PatientAllergy : BaseEntity
{
    public Guid PatientId { get; set; }
    public string AllergyName { get; set; } = null!;
    public AllergySeverity Severity { get; set; }
    public string? Reaction { get; set; }
    public DateTime? DiagnosedAt { get; set; }
    public string? Notes { get; set; }

    public Patient Patient { get; set; } = null!;
}

public enum AllergySeverity
{
    Mild,
    Moderate,
    Severe,
    LifeThreatening
}
```

---

## 📊 Final Model Assessment

### Flexibility: 10/10 ✅

- Supports clinics with/without pharmacy
- Supports clinics with/without lab
- Supports walk-ins and appointments
- Supports medicine refills without visits

### Business Logic: 9/10 ✅

- Prescription = Instructions (text) ✅
- Dispensing = Actual medicine given (optional) ✅
- Lab tests = Mostly external ✅
- Invoicing = Flexible for any scenario ✅

### Safety: 9/10 ✅

- Patient allergies tracked ✅
- Emergency contacts ✅
- Blood type ✅

---

## 🎯 Implementation Priority

1. **Add MedicineDispensing entity** (enables pharmacy feature)
2. **Add Patient allergies** (safety critical)
3. **Update InvoiceItem** (link to dispensing)
4. **Update LabTest status** (track workflow)
5. **Make relationships nullable** (flexibility)

Should I start implementing these changes?
