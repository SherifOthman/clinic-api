# Implementation Progress

## ✅ Completed:

### 1. MedicineDispensing Entity

- ✅ Created `MedicineDispensing` aggregate root
- ✅ Factory method `Create()`
- ✅ Behavior methods: `Cancel()`, `MarkAsPartiallyDispensed()`
- ✅ Domain events: `MedicineDispensedEvent`, `MedicineDispensingCancelledEvent`
- ✅ Supports dispensing with/without visit
- ✅ Supports dispensing with/without prescription

### 2. LabTestOrder Entity

- ✅ Created `LabTestOrder` aggregate root (renamed from MedicalVisitLabTest)
- ✅ Factory methods: `CreateFromVisit()`, `CreateStandalone()`
- ✅ Behavior methods: `MarkAsPerformed()`, `UploadResults()`, `Review()`, `Cancel()`
- ✅ Domain events: `LabTestOrderedEvent`, `LabTestResultsAvailableEvent`, `LabTestReviewedEvent`
- ✅ Supports tests with/without visit
- ✅ Supports tests with/without doctor order

## 🔄 In Progress:

### 3. RadiologyOrder Entity

- Need to create (same pattern as LabTestOrder)

### 4. Patient Allergies

- Need to add fields to Patient entity
- Need to create PatientAllergy entity

### 5. Update Appointment

- Need to remove payment tracking
- Need to add InvoiceId link

### 6. Update Invoice

- Need to add AppointmentId link

### 7. Update InvoiceItem

- Need to add MedicineDispensingId
- Need to add LabTestOrderId
- Need to add RadiologyOrderId

## ⏭️ Remaining:

### 8. Create RadiologyOrder Entity

### 9. Create PatientAllergy Entity

### 10. Update Patient Entity

### 11. Update Appointment Entity (remove payments)

### 12. Update Invoice Entity (add AppointmentId)

### 13. Update InvoiceItem Entity (add new links)

### 14. Create Database Migration

### 15. Update Handlers

### 16. Update Tests

---

## Next Steps:

Continue with RadiologyOrder, then Patient updates, then Appointment/Invoice updates.

The model is taking shape! All new entities follow DDD patterns with:

- Aggregate roots
- Factory methods
- Behavior methods
- Domain events
- Private setters
- Business rule enforcement
