# Implementation Plan - Clinic Model Fixes

## Changes to Implement:

### 1. Remove Payment Tracking from Appointment ✅

- Remove: FinalPrice, DiscountAmount, PaidAmount
- Remove: RecordPayment(), ApplyDiscount(), UpdatePrice()
- Add: InvoiceId (link to invoice)
- Add: Calculated property IsConsultationFeePaid

### 2. Create MedicineDispensing Entity ✅

- New aggregate root
- Tracks actual medicine dispensed
- Links to Medicine, Patient, Visit (optional), Prescription (optional)
- Domain events: MedicineDispensedEvent, MedicineDispensingCancelledEvent

### 3. Rename MedicalVisitLabTest → LabTestOrder ✅

- Make MedicalVisitId nullable
- Add OrderedByDoctorId (nullable)
- Add factory methods: CreateFromVisit(), CreateStandalone()
- Add behavior methods: MarkAsPerformed(), UploadResults(), Review()
- Domain events: LabTestOrderedEvent, LabTestResultsAvailableEvent, LabTestReviewedEvent

### 4. Create RadiologyOrder Entity ✅

- Same pattern as LabTestOrder
- Tracks radiology tests (X-ray, CT, MRI, etc.)
- Domain events: RadiologyOrderedEvent, RadiologyResultsAvailableEvent, RadiologyReviewedEvent

### 5. Update InvoiceItem ✅

- Add: MedicineDispensingId
- Add: LabTestOrderId
- Add: RadiologyOrderId
- Keep existing: MedicalServiceId, MedicalSupplyId

### 6. Add Patient Allergies ✅

- Add to Patient: BloodType, KnownAllergies (text)
- Add to Patient: EmergencyContactName, EmergencyContactPhone, EmergencyContactRelation
- Create PatientAllergy entity

### 7. Update Invoice ✅

- Add: AppointmentId (link to appointment)
- Keep: MedicalVisitId (nullable)

### 8. Update Domain Events ✅

- Remove payment events from Appointment
- Add new events for dispensing, lab, radiology

---

## Breaking Changes:

### Database Schema Changes:

1. Appointment table: Remove FinalPrice, DiscountAmount, PaidAmount columns
2. Appointment table: Add InvoiceId column
3. Create MedicineDispensings table
4. Rename MedicalVisitLabTests → LabTestOrders
5. LabTestOrders: Make MedicalVisitId nullable, Add OrderedByDoctorId
6. Create RadiologyOrders table
7. InvoiceItems: Add MedicineDispensingId, LabTestOrderId, RadiologyOrderId
8. Invoices: Add AppointmentId
9. Patients: Add BloodType, KnownAllergies, Emergency contact fields
10. Create PatientAllergies table

### Code Changes:

1. Update Appointment aggregate (remove payment methods)
2. Update AppointmentCreatedEvent (remove FinalPrice)
3. Update AppointmentCompletedEvent (remove payment fields)
4. Remove AppointmentPaymentRecordedEvent
5. Update all handlers that create appointments
6. Update all handlers that record payments (use Invoice instead)

---

## Migration Strategy:

### Step 1: Add New Entities (Non-Breaking)

- Create MedicineDispensing
- Create RadiologyOrder
- Create PatientAllergy
- Add new columns to existing tables

### Step 2: Data Migration

- For existing appointments with payments:
  - Create Invoice for each appointment
  - Add consultation fee as InvoiceItem
  - Migrate payment data to Invoice
  - Link Invoice to Appointment

### Step 3: Remove Old Columns (Breaking)

- Remove payment columns from Appointment
- Remove payment methods from Appointment

---

## Implementation Order:

1. ✅ Create new entities (no breaking changes yet)
2. ✅ Update Invoice to support appointments
3. ✅ Create migration for new tables
4. ✅ Update handlers to use new model
5. ✅ Data migration script
6. ✅ Remove old payment code from Appointment
7. ✅ Update tests

Let's start!
