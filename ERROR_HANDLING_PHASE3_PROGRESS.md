# Error Handling Migration - Phase 3 In Progress

## Phase 3: Handlers - IN PROGRESS

### What Has Been Done So Far

✅ **Updated 6 handler files:**

1. `Features/Patients/Queries/GetPatientById/GetPatientByIdQuery.cs`
2. `Features/Specializations/Queries/GetSpecializationById/GetSpecializationByIdQueryHandler.cs`
3. `Features/Medicines/Queries/GetMedicineById/GetMedicineByIdQueryHandler.cs`
4. `Features/Auth/Commands/ChangePassword/ChangePasswordCommandHandler.cs`
5. `Features/Auth/Commands/ResetPassword/ResetPasswordCommandHandler.cs`
6. `Features/Auth/Queries/GetMe/GetMeQuery.cs`

### Current Build Status

```
Build succeeded with 346 obsolete warnings
    0 Error(s)
```

---

## Remaining Work

### Files Still Need Updating (~44 files)

Based on grep search, here are the main categories:

#### Auth Commands/Queries (~15 files):

- `Features/Auth/Commands/Register/RegisterCommandHandler.cs`
- `Features/Auth/Commands/ConfirmEmail/ConfirmEmailCommandHandler.cs`
- `Features/Auth/Commands/ResendEmailVerification/ResendEmailVerificationCommandHandler.cs`
- `Features/Auth/Commands/UploadProfileImage/UploadProfileImageCommandHandler.cs`
- `Features/Auth/Commands/UpdateProfileImage/UpdateProfileImageCommand.cs`
- `Features/Auth/Commands/DeleteProfileImage/DeleteProfileImageCommand.cs`
- `Features/Auth/Commands/UpdateProfile/UpdateProfileCommand.cs`
- `Features/Auth/Commands/Login/LoginCommandHandler.cs`

#### Patient Commands (~8 files):

- `Features/Patients/Commands/AddChronicDisease/AddChronicDiseaseCommand.cs`
- `Features/Patients/Commands/UpdateChronicDisease/UpdateChronicDiseaseCommand.cs`
- `Features/Patients/Commands/RemoveChronicDisease/RemoveChronicDiseaseCommand.cs`
- `Features/Patients/Commands/DeletePatient/DeletePatientCommand.cs`
- `Features/Patients/Commands/UpdatePatient/UpdatePatientCommand.cs`

#### Medicine Commands (~5 files):

- `Features/Medicines/Commands/CreateMedicine/CreateMedicineCommand.cs`
- `Features/Medicines/Commands/UpdateMedicine/UpdateMedicineCommand.cs`
- `Features/Medicines/Commands/DeleteMedicine/DeleteMedicineCommand.cs`

#### Appointment Commands (~3 files):

- `Features/Appointments/Commands/CreateAppointment/CreateAppointmentCommand.cs`

#### Invoice Commands (~3 files):

- `Features/Invoices/Commands/CreateInvoice/CreateInvoiceCommand.cs`

#### Payment Commands (~2 files):

- `Features/Payments/Commands/CreatePayment/CreatePaymentCommand.cs`

#### Other Commands (~8 files):

- `Features/Onboarding/Commands/CompleteOnboarding/CompleteOnboardingCommand.cs`
- `Features/StaffInvitations/Commands/InviteStaff/InviteStaffCommand.cs`
- `Features/StaffInvitations/Commands/AcceptInvitation/AcceptInvitationCommand.cs`
- `Features/MedicalServices/Commands/CreateMedicalService/CreateMedicalServiceCommand.cs`
- `Features/MedicalSupplies/Commands/CreateMedicalSupply/CreateMedicalSupplyCommand.cs`
- `Features/Measurements/Commands/CreateMeasurementAttribute/CreateMeasurementAttributeCommand.cs`
- `Features/ChronicDiseases/Commands/UpdateChronicDisease/UpdateChronicDiseaseCommand.cs`
- `Features/ChronicDiseases/Commands/DeleteChronicDisease/DeleteChronicDiseaseCommand.cs`

---

## Migration Patterns

### Pattern 1: NOT_FOUND Errors → FailSystem

**Before:**

```csharp
if (entity == null)
    return Result<T>.Fail(MessageCodes.Patient.NOT_FOUND);
```

**After:**

```csharp
if (entity == null)
    return Result<T>.FailSystem("NOT_FOUND", "Patient not found");
```

### Pattern 2: Authentication Errors → FailSystem

**Before:**

```csharp
if (userId == null)
    return Result.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
```

**After:**

```csharp
if (userId == null)
    return Result.FailSystem("UNAUTHENTICATED", "User is not authenticated");
```

### Pattern 3: Validation Errors → FailValidation

**Before:**

```csharp
return Result.FailField("email", MessageCodes.Validation.EMAIL_ALREADY_REGISTERED);
```

**After:**

```csharp
return Result.FailValidation("email", "Email is already registered");
```

### Pattern 4: Business Logic Errors → FailBusiness

**Before:**

```csharp
if (medicine.Stock < quantity)
    return Result.Fail(MessageCodes.Medicine.INSUFFICIENT_STOCK);
```

**After:**

```csharp
if (medicine.Stock < quantity)
    return Result.FailBusiness(
        "INSUFFICIENT_STOCK",
        $"Insufficient stock for {medicine.Name}",
        new { medicineId = medicine.Id, available = medicine.Stock, requested = quantity });
```

### Pattern 5: System Errors → FailSystem

**Before:**

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error...");
    return Result.Fail(MessageCodes.Exception.INTERNAL_SERVER_ERROR);
}
```

**After:**

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error...");
    return Result.FailSystem("INTERNAL_ERROR", "An error occurred while processing your request");
}
```

---

## Decision Guide

### When to use FailSystem (with code):

- Entity not found (NOT_FOUND)
- User not authenticated (UNAUTHENTICATED)
- Unauthorized access (UNAUTHORIZED)
- Internal server errors (INTERNAL_ERROR)

### When to use FailBusiness (with code + metadata):

- Insufficient stock
- Duplicate entity
- Invalid state transition
- Business rule violation
- Any error that needs specific UI handling

### When to use FailValidation (no code):

- Field-level validation errors
- Format validation
- Required field missing
- Length validation
- Any error that can be handled generically by frontend

---

## Systematic Approach

### Step 1: Update Simple Query Handlers (DONE)

- ✅ GetPatientById
- ✅ GetMedicineById
- ✅ GetSpecializationById
- ✅ GetMe

### Step 2: Update Simple Auth Handlers (DONE)

- ✅ ChangePassword
- ✅ ResetPassword

### Step 3: Update Remaining Auth Handlers (TODO)

- Register
- ConfirmEmail
- ResendEmailVerification
- UploadProfileImage
- UpdateProfileImage
- DeleteProfileImage
- UpdateProfile
- Login

### Step 4: Update Patient Handlers (TODO)

- AddChronicDisease
- UpdateChronicDisease
- RemoveChronicDisease
- DeletePatient
- UpdatePatient

### Step 5: Update Medicine Handlers (TODO)

- CreateMedicine
- UpdateMedicine
- DeleteMedicine

### Step 6: Update Other Handlers (TODO)

- Appointments
- Invoices
- Payments
- Onboarding
- StaffInvitations
- MedicalServices
- MedicalSupplies
- Measurements
- ChronicDiseases

---

## Estimated Effort

- **Completed:** 6 files (~15 references)
- **Remaining:** ~44 files (~185 references)
- **Total:** ~50 files (~200 references)

**Time estimate:** 2-3 hours for remaining files

---

## Next Steps

### Option A: Continue Manually

Continue updating files one by one following the patterns above.

### Option B: Batch Update by Category

Update all files in one category at a time (e.g., all Auth handlers, then all Patient handlers).

### Option C: Pause and Test

Test the changes made so far before continuing.

---

## Recommendation

**Option B** - Batch update by category:

1. Easier to maintain consistency
2. Can test each category independently
3. Natural grouping by feature
4. Can delegate to team members if needed

---

## Testing Strategy

After updating each category:

1. Build the project
2. Run unit tests
3. Check warning count reduction
4. Test API endpoints manually
5. Verify frontend integration

---

## Summary

✅ Phase 1 Complete - Domain layer (~150 codes removed)
✅ Phase 2 Complete - Validators (~100 codes removed)
⏳ Phase 3 In Progress - Handlers (6/50 files, ~15/200 references)
⏳ ~185 code references remaining in handlers
⏳ Phase 4 Not Started - Infrastructure services
⏳ Phase 5 Not Started - Cleanup

Current build: 0 errors, 346 obsolete warnings (expected)
