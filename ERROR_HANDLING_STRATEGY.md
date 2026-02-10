# Error Handling Strategy - Simplification Proposal

## Current Problem

We have **200+ message codes** scattered across the application, which creates several issues:

### Issues:

1. **Over-Engineering** ❌
   - Too many granular codes (e.g., `FIELD.FIRST_NAME.MIN_LENGTH`, `FIELD.FIRST_NAME.MAX_LENGTH`)
   - Frontend doesn't need this level of detail
   - Creates maintenance burden

2. **Duplication** ❌
   - Similar codes in multiple places
   - `VALIDATION.EMAIL.REQUIRED` vs `FIELD.EMAIL.REQUIRED`
   - `DOMAIN.PATIENT.VALIDATION_FAILED` vs `PATIENT.VALIDATION_FAILED`

3. **Inconsistency** ❌
   - Some errors use codes, some don't
   - No clear pattern for when to use codes

4. **Frontend Complexity** ❌
   - Frontend needs to handle 200+ codes
   - Most codes just map to generic messages anyway
   - Increases bundle size with translation files

---

## Recommended Approach

### Principle: **Backend Validates, Frontend Displays**

**Backend Responsibility:**

- ✅ Validate all data for security and consistency
- ✅ Return structured errors with field names
- ✅ Use codes ONLY for business logic errors that need specific UI handling

**Frontend Responsibility:**

- ✅ Display validation errors generically
- ✅ Handle specific business errors with custom UI
- ✅ Provide user-friendly messages

---

## Proposed Solution

### 1. Simplify Message Codes (Keep Only ~30 codes)

**Keep codes for:**

- Business logic errors that need specific UI handling
- Authentication/Authorization errors
- Critical system errors

**Remove codes for:**

- Field validation errors (use generic validation response)
- Duplicate/similar codes
- Over-granular codes

### Simplified MessageCodes:

```csharp
public static class MessageCodes
{
    // Authentication (Keep - needs specific UI handling)
    public static class Auth
    {
        public const string INVALID_CREDENTIALS = "AUTH.INVALID_CREDENTIALS";
        public const string EMAIL_NOT_CONFIRMED = "AUTH.EMAIL_NOT_CONFIRMED";
        public const string UNAUTHORIZED = "AUTH.UNAUTHORIZED";
        public const string SESSION_EXPIRED = "AUTH.SESSION_EXPIRED";
    }

    // Business Logic (Keep - needs specific UI handling)
    public static class Business
    {
        public const string INSUFFICIENT_STOCK = "BUSINESS.INSUFFICIENT_STOCK";
        public const string APPOINTMENT_CONFLICT = "BUSINESS.APPOINTMENT_CONFLICT";
        public const string INVOICE_ALREADY_PAID = "BUSINESS.INVOICE_ALREADY_PAID";
        public const string PATIENT_HAS_ALLERGY = "BUSINESS.PATIENT_HAS_ALLERGY";
        public const string MEDICINE_EXPIRED = "BUSINESS.MEDICINE_EXPIRED";
        public const string PAYMENT_EXCEEDS_AMOUNT = "BUSINESS.PAYMENT_EXCEEDS_AMOUNT";
    }

    // System Errors (Keep - needs specific UI handling)
    public static class System
    {
        public const string NOT_FOUND = "SYSTEM.NOT_FOUND";
        public const string INTERNAL_ERROR = "SYSTEM.INTERNAL_ERROR";
        public const string SERVICE_UNAVAILABLE = "SYSTEM.SERVICE_UNAVAILABLE";
    }

    // Validation (Remove codes - use generic response)
    // NO CODES NEEDED - Frontend handles generically
}
```

### 2. New Error Response Structure

**For Validation Errors (No Codes):**

```csharp
// Backend returns
{
  "success": false,
  "errors": {
    "email": ["Email is required", "Email format is invalid"],
    "password": ["Password must be at least 6 characters"],
    "firstName": ["First name is required"]
  }
}

// Frontend displays
<input name="email" />
{errors.email && <span>{errors.email[0]}</span>}
// OR use generic message
{errors.email && <span>Please check this field</span>}
```

**For Business Logic Errors (With Codes):**

```csharp
// Backend returns
{
  "success": false,
  "code": "BUSINESS.INSUFFICIENT_STOCK",
  "message": "Insufficient stock for Medicine X",
  "data": {
    "medicineId": "123",
    "medicineName": "Paracetamol",
    "requested": 50,
    "available": 20
  }
}

// Frontend handles specifically
if (error.code === 'BUSINESS.INSUFFICIENT_STOCK') {
  showStockWarningDialog(error.data);
} else {
  showGenericError(error.message);
}
```

---

## Implementation Plan

### Phase 1: Update Result Class

```csharp
public class Result<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public string? Code { get; init; }  // Only for business logic errors
    public Dictionary<string, List<string>>? Errors { get; init; }  // For validation errors
    public object? Metadata { get; init; }  // For additional context

    // For validation errors (no code)
    public static Result<T> FailValidation(Dictionary<string, List<string>> errors)
    {
        return new Result<T>
        {
            Success = false,
            Errors = errors
        };
    }

    // For business logic errors (with code)
    public static Result<T> FailBusiness(string code, string message, object? metadata = null)
    {
        return new Result<T>
        {
            Success = false,
            Code = code,
            Message = message,
            Metadata = metadata
        };
    }

    // For system errors (with code)
    public static Result<T> FailSystem(string code, string message)
    {
        return new Result<T>
        {
            Success = false,
            Code = code,
            Message = message
        };
    }
}
```

### Phase 2: Update Validators (Remove Codes)

**Before:**

```csharp
RuleFor(x => x.Email)
    .NotEmpty().WithMessage(MessageCodes.Fields.EMAIL_REQUIRED)
    .EmailAddress().WithMessage(MessageCodes.Fields.EMAIL_INVALID_FORMAT)
    .MaximumLength(255).WithMessage(MessageCodes.Fields.EMAIL_MAX_LENGTH);
```

**After:**

```csharp
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email is required")
    .EmailAddress().WithMessage("Email format is invalid")
    .MaximumLength(255).WithMessage("Email must not exceed 255 characters");
```

Frontend will display these messages OR use generic "Please check this field".

### Phase 3: Update Domain Exceptions (Keep Codes for Business Logic)

**Before:**

```csharp
throw new InvalidBusinessOperationException(
    "First name is required",
    MessageCodes.Fields.FIRST_NAME_REQUIRED);  // ❌ Too granular
```

**After:**

```csharp
// For validation - no code needed
throw new InvalidBusinessOperationException("First name is required");

// For business logic - use code
throw new InsufficientStockException(
    "BUSINESS.INSUFFICIENT_STOCK",
    $"Insufficient stock for {medicineName}",
    new { medicineId, requested, available });
```

### Phase 4: Update Handlers

**Before:**

```csharp
if (patient == null)
    return Result<PatientDto>.Fail(MessageCodes.Patient.NOT_FOUND);
```

**After:**

```csharp
// System error - use code
if (patient == null)
    return Result<PatientDto>.FailSystem("SYSTEM.NOT_FOUND", "Patient not found");

// Business error - use code
if (medicine.Stock < quantity)
    return Result<OrderDto>.FailBusiness(
        "BUSINESS.INSUFFICIENT_STOCK",
        $"Insufficient stock for {medicine.Name}",
        new { medicineId = medicine.Id, available = medicine.Stock, requested = quantity });

// Validation error - no code
if (validationResult.IsInvalid)
    return Result<PatientDto>.FailValidation(validationResult.Errors);
```

---

## Benefits

### 1. Simpler Codebase ✅

- **200+ codes → ~30 codes** (85% reduction)
- Less maintenance
- Clearer code

### 2. Better Frontend Experience ✅

- Generic validation handling
- Specific business logic handling
- Smaller translation files

### 3. Flexibility ✅

- Frontend can choose to display field errors or generic messages
- Easy to add new business logic codes when needed
- No need to update codes for every validation rule

### 4. Consistency ✅

- Clear pattern: codes for business logic, no codes for validation
- Easy to understand and follow

---

## Examples

### Example 1: Create Patient (Validation Error)

**Backend:**

```csharp
public async Task<Result<PatientDto>> Handle(CreatePatientCommand request, CancellationToken ct)
{
    // Validation happens automatically via FluentValidation
    // If validation fails, returns:
    return Result<PatientDto>.FailValidation(new Dictionary<string, List<string>>
    {
        ["email"] = ["Email is required"],
        ["firstName"] = ["First name must be at least 2 characters"],
        ["dateOfBirth"] = ["Date of birth cannot be in the future"]
    });
}
```

**Frontend:**

```typescript
// Option 1: Display specific messages
{errors.email && <span className="error">{errors.email[0]}</span>}

// Option 2: Display generic message
{errors.email && <span className="error">Please check this field</span>}

// Option 3: Just highlight field
<input className={errors.email ? 'error' : ''} />
```

### Example 2: Create Appointment (Business Logic Error)

**Backend:**

```csharp
// Check for appointment conflict
if (await HasConflict(doctorId, appointmentDate, queueNumber))
{
    return Result<AppointmentDto>.FailBusiness(
        "BUSINESS.APPOINTMENT_CONFLICT",
        "This time slot is already booked",
        new
        {
            doctorId,
            appointmentDate,
            queueNumber,
            suggestedSlots = await GetAvailableSlots(doctorId, appointmentDate)
        });
}
```

**Frontend:**

```typescript
if (error.code === "BUSINESS.APPOINTMENT_CONFLICT") {
  // Show specific UI for appointment conflict
  showConflictDialog({
    message: error.message,
    suggestedSlots: error.metadata.suggestedSlots,
  });
} else {
  // Generic error
  showToast(error.message);
}
```

### Example 3: Dispense Medicine (Business Logic Error)

**Backend:**

```csharp
// Check stock
if (medicine.Stock < quantity)
{
    return Result<DispensingDto>.FailBusiness(
        "BUSINESS.INSUFFICIENT_STOCK",
        $"Insufficient stock for {medicine.Name}",
        new
        {
            medicineId = medicine.Id,
            medicineName = medicine.Name,
            requested = quantity,
            available = medicine.Stock,
            reorderLevel = medicine.ReorderLevel
        });
}

// Check allergy
if (patient.HasAllergy(medicine.ActiveIngredient))
{
    return Result<DispensingDto>.FailBusiness(
        "BUSINESS.PATIENT_HAS_ALLERGY",
        $"Patient is allergic to {medicine.ActiveIngredient}",
        new
        {
            patientId = patient.Id,
            allergyName = medicine.ActiveIngredient,
            severity = patient.GetAllergySeverity(medicine.ActiveIngredient)
        });
}
```

**Frontend:**

```typescript
if (error.code === "BUSINESS.INSUFFICIENT_STOCK") {
  showStockWarningDialog({
    medicine: error.metadata.medicineName,
    available: error.metadata.available,
    requested: error.metadata.requested,
  });
} else if (error.code === "BUSINESS.PATIENT_HAS_ALLERGY") {
  showAllergyWarningDialog({
    allergyName: error.metadata.allergyName,
    severity: error.metadata.severity,
  });
} else {
  showToast(error.message);
}
```

---

## Migration Strategy

### Step 1: Update Result Class (Non-Breaking)

- Add new methods: `FailValidation()`, `FailBusiness()`, `FailSystem()`
- Keep existing methods for backward compatibility

### Step 2: Update New Code

- Use new pattern for all new features
- Gradually update existing code

### Step 3: Remove Old Codes

- After all code updated, remove unused message codes
- Clean up MessageCodes class

---

## Conclusion

### Current State:

- ❌ 200+ message codes
- ❌ Over-engineered
- ❌ Maintenance burden
- ❌ Frontend complexity

### Proposed State:

- ✅ ~30 message codes (business logic only)
- ✅ Simple and clear
- ✅ Easy to maintain
- ✅ Better frontend experience

### Recommendation:

**Implement this strategy for new code immediately. Gradually migrate existing code.**

---

## Decision

**What do you think?**

1. **Option A:** Implement full simplification (remove 170+ codes)
2. **Option B:** Keep current approach (200+ codes)
3. **Option C:** Hybrid approach (simplify gradually)

**My recommendation:** Option A or C

The key insight is: **Frontend doesn't need granular validation codes. It needs:**

- Field names + error messages (for validation)
- Business logic codes + context (for specific UI handling)
