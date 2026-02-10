# Services Without Visit - Complete Model

## 🎯 Key Insight: ANY Service Can Happen Without a Visit!

### Real Scenarios:

1. **Medicine refill** - Patient gets chronic disease medicine
2. **Lab test** - Patient comes for blood test only
3. **Radiology** - Patient comes for X-ray only
4. **Follow-up** - Patient brings external results

**All without seeing a doctor!**

---

## 🔄 Revised Entity Model

### 1. MedicineDispensing (Already Designed)

```csharp
public class MedicineDispensing : AggregateRoot
{
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid MedicineId { get; set; }

    // Optional links
    public Guid? VisitId { get; set; }  // ✅ Null if no visit
    public Guid? PrescriptionId { get; set; }  // ✅ Null if OTC or refill

    public int Quantity { get; set; }
    public SaleUnit Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid DispensedByUserId { get; set; }
    public DateTime DispensedAt { get; set; }
}
```

**Use Cases:**

- ✅ With visit: Doctor prescribes → Pharmacy dispenses
- ✅ Without visit: Patient refills chronic medicine
- ✅ OTC: Patient buys over-the-counter medicine

---

### 2. LabTestOrder (Renamed from MedicalVisitLabTest)

**CRITICAL CHANGE**: Decouple from MedicalVisit!

```csharp
public class LabTestOrder : AggregateRoot
{
    private LabTestOrder() { }

    public Guid ClinicBranchId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid LabTestId { get; private set; }

    // Optional links
    public Guid? MedicalVisitId { get; private set; }  // ✅ Null if no visit
    public Guid? OrderedByDoctorId { get; private set; }  // ✅ Null if patient self-requested

    // Workflow
    public LabTestStatus Status { get; private set; }
    public DateTime OrderedAt { get; private set; }

    // Performing (if done internally)
    public DateTime? PerformedAt { get; private set; }
    public Guid? PerformedByUserId { get; private set; }

    // Results
    public DateTime? ResultsAvailableAt { get; private set; }
    public Guid? ResultsUploadedByUserId { get; private set; }
    public string? ResultsFilePath { get; private set; }
    public string? ResultsText { get; private set; }
    public bool? IsAbnormal { get; private set; }

    // Review
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedByDoctorId { get; private set; }
    public string? DoctorNotes { get; private set; }

    public string? Notes { get; private set; }

    // Navigation
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public LabTest LabTest { get; set; } = null!;
    public MedicalVisit? MedicalVisit { get; set; }
    public Doctor? OrderedByDoctor { get; set; }

    /// <summary>
    /// Factory: Doctor orders test during visit
    /// </summary>
    public static LabTestOrder CreateFromVisit(
        Guid clinicBranchId,
        Guid patientId,
        Guid labTestId,
        Guid medicalVisitId,
        Guid orderedByDoctorId,
        string? notes = null)
    {
        var order = new LabTestOrder
        {
            ClinicBranchId = clinicBranchId,
            PatientId = patientId,
            LabTestId = labTestId,
            MedicalVisitId = medicalVisitId,
            OrderedByDoctorId = orderedByDoctorId,
            Status = LabTestStatus.Ordered,
            OrderedAt = DateTime.UtcNow,
            Notes = notes
        };

        order.AddDomainEvent(new LabTestOrderedEvent(
            order.Id,
            patientId,
            labTestId,
            medicalVisitId
        ));

        return order;
    }

    /// <summary>
    /// Factory: Patient requests test without visit
    /// </summary>
    public static LabTestOrder CreateStandalone(
        Guid clinicBranchId,
        Guid patientId,
        Guid labTestId,
        string? notes = null)
    {
        var order = new LabTestOrder
        {
            ClinicBranchId = clinicBranchId,
            PatientId = patientId,
            LabTestId = labTestId,
            MedicalVisitId = null,  // No visit
            OrderedByDoctorId = null,  // No doctor
            Status = LabTestStatus.Ordered,
            OrderedAt = DateTime.UtcNow,
            Notes = notes
        };

        order.AddDomainEvent(new LabTestOrderedEvent(
            order.Id,
            patientId,
            labTestId,
            null
        ));

        return order;
    }

    /// <summary>
    /// Mark test as performed (internal lab)
    /// </summary>
    public void MarkAsPerformed(Guid performedByUserId)
    {
        if (Status != LabTestStatus.Ordered)
            throw new InvalidBusinessOperationException("Test must be in Ordered status");

        Status = LabTestStatus.InProgress;
        PerformedAt = DateTime.UtcNow;
        PerformedByUserId = performedByUserId;
    }

    /// <summary>
    /// Upload results (internal or external)
    /// </summary>
    public void UploadResults(
        string? resultsFilePath,
        string? resultsText,
        bool? isAbnormal,
        Guid uploadedByUserId)
    {
        if (Status == LabTestStatus.Cancelled)
            throw new InvalidBusinessOperationException("Cannot upload results for cancelled test");

        Status = LabTestStatus.ResultsAvailable;
        ResultsAvailableAt = DateTime.UtcNow;
        ResultsFilePath = resultsFilePath;
        ResultsText = resultsText;
        IsAbnormal = isAbnormal;
        ResultsUploadedByUserId = uploadedByUserId;

        AddDomainEvent(new LabTestResultsAvailableEvent(
            Id,
            PatientId,
            LabTestId,
            isAbnormal ?? false
        ));
    }

    /// <summary>
    /// Doctor reviews results
    /// </summary>
    public void Review(Guid reviewedByDoctorId, string? doctorNotes)
    {
        if (Status != LabTestStatus.ResultsAvailable)
            throw new InvalidBusinessOperationException("Results must be available before review");

        Status = LabTestStatus.Reviewed;
        ReviewedAt = DateTime.UtcNow;
        ReviewedByDoctorId = reviewedByDoctorId;
        DoctorNotes = doctorNotes;

        AddDomainEvent(new LabTestReviewedEvent(
            Id,
            PatientId,
            reviewedByDoctorId
        ));
    }

    public void Cancel(string reason)
    {
        if (Status == LabTestStatus.Reviewed)
            throw new InvalidBusinessOperationException("Cannot cancel reviewed test");

        Status = LabTestStatus.Cancelled;
        Notes = reason;
    }
}

public enum LabTestStatus
{
    Ordered,           // Ordered (by doctor or patient)
    InProgress,        // Being performed (internal lab)
    ResultsAvailable,  // Results uploaded
    Reviewed,          // Doctor reviewed
    Cancelled          // Cancelled
}
```

**Use Cases:**

- ✅ Doctor orders during visit: `CreateFromVisit()`
- ✅ Patient comes for test only: `CreateStandalone()`
- ✅ Patient brings external results: `CreateStandalone()` + `UploadResults()`

---

### 3. RadiologyOrder (New Entity - Same Pattern)

```csharp
public class RadiologyOrder : AggregateRoot
{
    private RadiologyOrder() { }

    public Guid ClinicBranchId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid RadiologyTestId { get; private set; }

    // Optional links
    public Guid? MedicalVisitId { get; private set; }  // ✅ Null if no visit
    public Guid? OrderedByDoctorId { get; private set; }  // ✅ Null if patient self-requested

    // Workflow
    public RadiologyStatus Status { get; private set; }
    public DateTime OrderedAt { get; private set; }

    // Performing (if done internally)
    public DateTime? PerformedAt { get; private set; }
    public Guid? PerformedByUserId { get; private set; }

    // Results (images)
    public DateTime? ResultsAvailableAt { get; private set; }
    public Guid? ResultsUploadedByUserId { get; private set; }
    public string? ImageFilePath { get; private set; }
    public string? ReportFilePath { get; private set; }
    public string? Findings { get; private set; }

    // Review
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedByDoctorId { get; private set; }
    public string? DoctorNotes { get; private set; }

    public string? Notes { get; private set; }

    // Navigation
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public RadiologyTest RadiologyTest { get; set; } = null!;
    public MedicalVisit? MedicalVisit { get; set; }
    public Doctor? OrderedByDoctor { get; set; }

    // Same factory methods as LabTestOrder
    public static RadiologyOrder CreateFromVisit(...) { }
    public static RadiologyOrder CreateStandalone(...) { }

    // Same behavior methods
    public void MarkAsPerformed(...) { }
    public void UploadResults(...) { }
    public void Review(...) { }
    public void Cancel(...) { }
}

public enum RadiologyStatus
{
    Ordered,
    InProgress,
    ResultsAvailable,
    Reviewed,
    Cancelled
}
```

---

### 4. Updated Invoice to Support All Services

```csharp
public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }

    // Flexible item types (only ONE should be set)
    public Guid? MedicalServiceId { get; set; }  // Consultation, etc.
    public Guid? MedicineDispensingId { get; set; }  // Medicine sold
    public Guid? LabTestOrderId { get; set; }  // ✨ Lab test performed
    public Guid? RadiologyOrderId { get; set; }  // ✨ Radiology performed
    public Guid? MedicalSupplyId { get; set; }  // Supplies sold

    // For external services (no internal entity)
    public string? ItemDescription { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;

    // Navigation
    public Invoice Invoice { get; set; } = null!;
    public MedicalService? MedicalService { get; set; }
    public MedicineDispensing? MedicineDispensing { get; set; }
    public LabTestOrder? LabTestOrder { get; set; }  // ✨ NEW
    public RadiologyOrder? RadiologyOrder { get; set; }  // ✨ NEW
    public MedicalSupply? MedicalSupply { get; set; }
}
```

---

## 📋 Complete Workflows

### Workflow 1: Medicine Refill (No Visit)

```csharp
// 1. Patient comes to pharmacy
var dispensing = MedicineDispensing.Create(
    clinicBranchId: branchId,
    patientId: patientId,
    medicineId: medicineId,
    quantity: 30,
    unit: SaleUnit.Strip,
    unitPrice: medicine.StripPrice,
    dispensedByUserId: pharmacistId,
    visitId: null,  // ✅ No visit
    prescriptionId: oldPrescriptionId  // Reference old prescription
);

// 2. Deduct from inventory
medicine.RemoveStock(30, "Dispensed to patient");

// 3. Create invoice
var invoice = Invoice.Create(
    clinicId: clinicId,
    patientId: patientId,
    medicalVisitId: null  // ✅ No visit
);

invoice.AddItem(
    medicineDispensingId: dispensing.Id,
    quantity: 30,
    unitPrice: medicine.StripPrice
);

// 4. Patient pays
invoice.RecordPayment(totalAmount);

// Done! No doctor, no visit, no appointment
```

---

### Workflow 2: Lab Test Only (No Visit)

```csharp
// 1. Patient comes for lab test
var labOrder = LabTestOrder.CreateStandalone(
    clinicBranchId: branchId,
    patientId: patientId,
    labTestId: cbcTestId,
    notes: "Patient self-requested"
);

// 2. Lab tech performs test (if internal lab)
labOrder.MarkAsPerformed(labTechUserId);

// 3. Upload results
labOrder.UploadResults(
    resultsFilePath: "/uploads/lab-results/123.pdf",
    resultsText: "WBC: 7.5, RBC: 4.8...",
    isAbnormal: false,
    uploadedByUserId: labTechUserId
);

// 4. Create invoice
var invoice = Invoice.Create(
    clinicId: clinicId,
    patientId: patientId,
    medicalVisitId: null  // ✅ No visit
);

invoice.AddItem(
    labTestOrderId: labOrder.Id,
    quantity: 1,
    unitPrice: cbcTest.Price
);

// 5. Patient pays
invoice.RecordPayment(totalAmount);

// 6. Doctor reviews later (optional)
labOrder.Review(doctorId, "Results normal");
```

---

### Workflow 3: Radiology Only (No Visit)

```csharp
// 1. Patient comes for X-ray
var radiologyOrder = RadiologyOrder.CreateStandalone(
    clinicBranchId: branchId,
    patientId: patientId,
    radiologyTestId: chestXrayId
);

// 2. Radiologist performs X-ray
radiologyOrder.MarkAsPerformed(radiologistUserId);

// 3. Upload images and report
radiologyOrder.UploadResults(
    imageFilePath: "/uploads/xrays/123.jpg",
    reportFilePath: "/uploads/reports/123.pdf",
    findings: "No abnormalities detected",
    uploadedByUserId: radiologistUserId
);

// 4. Create invoice
var invoice = Invoice.Create(
    clinicId: clinicId,
    patientId: patientId,
    medicalVisitId: null  // ✅ No visit
);

invoice.AddItem(
    radiologyOrderId: radiologyOrder.Id,
    quantity: 1,
    unitPrice: chestXray.Price
);

// 5. Patient pays
invoice.RecordPayment(totalAmount);
```

---

### Workflow 4: Patient Brings External Results

```csharp
// 1. Patient: "Here are my lab results from external lab"
var labOrder = LabTestOrder.CreateStandalone(
    clinicBranchId: branchId,
    patientId: patientId,
    labTestId: cbcTestId,
    notes: "External lab results"
);

// 2. Staff uploads results
labOrder.UploadResults(
    resultsFilePath: "/uploads/external-results/123.pdf",
    resultsText: null,
    isAbnormal: null,
    uploadedByUserId: receptionistId
);

// 3. No invoice (patient already paid external lab)

// 4. Doctor reviews
labOrder.Review(doctorId, "Results reviewed, follow-up needed");
```

---

## 🎯 Database Schema Changes

### 1. Rename Table

```sql
-- Old
MedicalVisitLabTests

-- New
LabTestOrders
```

### 2. Make Columns Nullable

```sql
ALTER TABLE LabTestOrders
ALTER COLUMN MedicalVisitId uniqueidentifier NULL;

ALTER TABLE LabTestOrders
ALTER COLUMN OrderedByDoctorId uniqueidentifier NULL;
```

### 3. Add New Table

```sql
CREATE TABLE RadiologyOrders (
    Id uniqueidentifier PRIMARY KEY,
    ClinicBranchId uniqueidentifier NOT NULL,
    PatientId uniqueidentifier NOT NULL,
    RadiologyTestId uniqueidentifier NOT NULL,
    MedicalVisitId uniqueidentifier NULL,  -- ✅ Nullable
    OrderedByDoctorId uniqueidentifier NULL,  -- ✅ Nullable
    Status int NOT NULL,
    OrderedAt datetime2 NOT NULL,
    -- ... other fields
);
```

### 4. Add New Table

```sql
CREATE TABLE MedicineDispensings (
    Id uniqueidentifier PRIMARY KEY,
    ClinicBranchId uniqueidentifier NOT NULL,
    PatientId uniqueidentifier NOT NULL,
    MedicineId uniqueidentifier NOT NULL,
    VisitId uniqueidentifier NULL,  -- ✅ Nullable
    PrescriptionId uniqueidentifier NULL,  -- ✅ Nullable
    Quantity int NOT NULL,
    Unit int NOT NULL,
    UnitPrice decimal(18,2) NOT NULL,
    TotalPrice decimal(18,2) NOT NULL,
    DispensedByUserId uniqueidentifier NOT NULL,
    DispensedAt datetime2 NOT NULL,
    Status int NOT NULL,
    -- ... other fields
);
```

### 5. Update InvoiceItems

```sql
ALTER TABLE InvoiceItems
ADD MedicineDispensingId uniqueidentifier NULL;

ALTER TABLE InvoiceItems
ADD LabTestOrderId uniqueidentifier NULL;

ALTER TABLE InvoiceItems
ADD RadiologyOrderId uniqueidentifier NULL;
```

---

## ✅ Final Model Assessment

### Flexibility: 10/10 ✅

- ✅ Medicine without visit
- ✅ Lab test without visit
- ✅ Radiology without visit
- ✅ Any combination of services
- ✅ External results upload

### Business Logic: 10/10 ✅

- ✅ Prescription = Instructions (text)
- ✅ Dispensing = Actual medicine (optional)
- ✅ Lab/Radiology = Flexible (with/without visit)
- ✅ Invoice = Any service combination

### Real Clinic Support: 10/10 ✅

- ✅ Consultation-only clinics
- ✅ Clinics with pharmacy
- ✅ Clinics with lab/radiology
- ✅ Full-service clinics
- ✅ Chronic patient refills
- ✅ Walk-in services

---

## 🎯 Implementation Order

1. **MedicineDispensing entity** (pharmacy feature)
2. **Rename MedicalVisitLabTest → LabTestOrder** (make flexible)
3. **Create RadiologyOrder entity** (same pattern)
4. **Update InvoiceItem** (link all service types)
5. **Add Patient allergies** (safety)
6. **Migration** (database changes)

Ready to implement?
