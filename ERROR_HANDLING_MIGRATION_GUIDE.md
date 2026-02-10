# Error Handling Migration Guide

## Overview

We're simplifying error handling from **200+ message codes to ~30 codes**. This guide shows how to migrate existing code.

---

## Quick Reference

### Old Approach (Deprecated):

```csharp
// ❌ Old - Too many codes
return Result<T>.Fail(MessageCodes.Fields.EMAIL_REQUIRED);
return Result<T>.Fail(MessageCodes.Validation.EMAIL_INVALID_FORMAT);
return Result<T>.FailField("email", MessageCodes.Fields.EMAIL_REQUIRED);
```

### New Approach:

```csharp
// ✅ New - Validation errors (no code)
return Result<T>.FailValidation("email", "Email is required");
return Result<T>.FailValidation(new Dictionary<string, List<string>>
{
    ["email"] = ["Email is required", "Email format is invalid"],
    ["password"] = ["Password must be at least 6 characters"]
});

// ✅ New - Business logic errors (with code)
return Result<T>.FailBusiness(
    MessageCodes.Business.INSUFFICIENT_STOCK,
    $"Insufficient stock for {medicineName}",
    new { medicineId, available, requested });

// ✅ New - System errors (with code)
return Result<T>.FailSystem(
    MessageCodes.System.NOT_FOUND,
    "Patient not found");
```

---

## Migration Patterns

### Pattern 1: Domain Validation → No Code

**Before:**

```csharp
if (string.IsNullOrWhiteSpace(name))
    throw new InvalidBusinessOperationException(
        "Medicine name is required",
        MessageCodes.Domain.MEDICINE_VALIDATION_FAILED);
```

**After:**

```csharp
if (string.IsNullOrWhiteSpace(name))
    throw new InvalidBusinessOperationException("Medicine name is required");
```

### Pattern 2: FluentValidation → No Code

**Before:**

```csharp
RuleFor(x => x.Email)
    .NotEmpty().WithMessage(MessageCodes.Fields.EMAIL_REQUIRED)
    .EmailAddress().WithMessage(MessageCodes.Fields.EMAIL_INVALID_FORMAT);
```

**After:**

```csharp
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email is required")
    .EmailAddress().WithMessage("Email format is invalid");
```

### Pattern 3: Handler Validation → FailValidation

**Before:**

```csharp
if (patient == null)
    return Result<PatientDto>.Fail(MessageCodes.Patient.NOT_FOUND);
```

**After:**

```csharp
if (patient == null)
    return Result<PatientDto>.FailSystem(
        MessageCodes.System.NOT_FOUND,
        "Patient not found");
```

### Pattern 4: Business Logic → FailBusiness

**Before:**

```csharp
if (medicine.Stock < quantity)
    return Result<OrderDto>.Fail(MessageCodes.Medicine.INSUFFICIENT_STOCK);
```

**After:**

```csharp
if (medicine.Stock < quantity)
    return Result<OrderDto>.FailBusiness(
        MessageCodes.Business.INSUFFICIENT_STOCK,
        $"Insufficient stock for {medicine.Name}",
        new
        {
            medicineId = medicine.Id,
            medicineName = medicine.Name,
            available = medicine.Stock,
            requested = quantity
        });
```

### Pattern 5: Multiple Field Errors → FailValidation

**Before:**

```csharp
return Result<T>.Fail(
    new ErrorItem("email", MessageCodes.Fields.EMAIL_REQUIRED),
    new ErrorItem("password", MessageCodes.Fields.PASSWORD_REQUIRED)
);
```

**After:**

```csharp
return Result<T>.FailValidation(new Dictionary<string, List<string>>
{
    ["email"] = ["Email is required"],
    ["password"] = ["Password is required"]
});
```

---

## Code Mapping Table

| Old Code                                        | New Approach                               | Example        |
| ----------------------------------------------- | ------------------------------------------ | -------------- |
| `MessageCodes.Fields.*`                         | `FailValidation(field, message)`           | No code needed |
| `MessageCodes.Validation.*`                     | `FailValidation(field, message)`           | No code needed |
| `MessageCodes.Domain.PATIENT_VALIDATION_FAILED` | `throw new Exception(message)`             | No code needed |
| `MessageCodes.Medicine.INSUFFICIENT_STOCK`      | `MessageCodes.Business.INSUFFICIENT_STOCK` | Use new code   |
| `MessageCodes.Patient.NOT_FOUND`                | `MessageCodes.System.NOT_FOUND`            | Use new code   |
| `MessageCodes.Authentication.*`                 | `MessageCodes.Auth.*`                      | Use new code   |

---

## Step-by-Step Migration

### Step 1: Update Domain Entities

**Files to update:**

- `Domain/Entities/**/*.cs`
- `Domain/Common/ValueObjects/*.cs`

**Changes:**

- Remove `MessageCodes.Domain.*` from exceptions
- Keep message text, remove code parameter

**Example:**

```csharp
// Before
throw new InvalidBusinessOperationException(
    "Patient code is required",
    MessageCodes.Domain.PATIENT_VALIDATION_FAILED);

// After
throw new InvalidBusinessOperationException("Patient code is required");
```

### Step 2: Update Validators

**Files to update:**

- `Application/Features/**/Validators/*.cs`

**Changes:**

- Remove `MessageCodes.Fields.*` and `MessageCodes.Validation.*`
- Use plain text messages

**Example:**

```csharp
// Before
RuleFor(x => x.FirstName)
    .NotEmpty().WithMessage(MessageCodes.Fields.FIRST_NAME_REQUIRED)
    .MinimumLength(2).WithMessage(MessageCodes.Fields.FIRST_NAME_MIN_LENGTH);

// After
RuleFor(x => x.FirstName)
    .NotEmpty().WithMessage("First name is required")
    .MinimumLength(2).WithMessage("First name must be at least 2 characters");
```

### Step 3: Update Handlers

**Files to update:**

- `Application/Features/**/Commands/**/*Handler.cs`
- `Application/Features/**/Queries/**/*Handler.cs`

**Changes:**

- Use `FailValidation()` for validation errors
- Use `FailBusiness()` for business logic errors
- Use `FailSystem()` for system errors

**Example:**

```csharp
// Before
if (patient == null)
    return Result<PatientDto>.Fail(MessageCodes.Patient.NOT_FOUND);

if (medicine.Stock < quantity)
    return Result<OrderDto>.Fail(MessageCodes.Medicine.INSUFFICIENT_STOCK);

// After
if (patient == null)
    return Result<PatientDto>.FailSystem(
        MessageCodes.System.NOT_FOUND,
        "Patient not found");

if (medicine.Stock < quantity)
    return Result<OrderDto>.FailBusiness(
        MessageCodes.Business.INSUFFICIENT_STOCK,
        $"Insufficient stock for {medicine.Name}",
        new { medicineId = medicine.Id, available = medicine.Stock, requested = quantity });
```

### Step 4: Update API Controllers

**Files to update:**

- `API/Controllers/*.cs`

**Changes:**

- Update error response mapping
- Handle new Result structure

**Example:**

```csharp
// Before
if (result.IsFailure)
    return BadRequest(new { code = result.Code });

// After
if (result.IsFailure)
{
    if (result.HasValidationErrors)
        return BadRequest(new { errors = result.ValidationErrors });

    if (result.HasBusinessError)
        return BadRequest(new
        {
            code = result.Code,
            message = result.Message,
            metadata = result.Metadata
        });

    return StatusCode(500, new { message = result.Message });
}
```

---

## Testing Migration

### Test Each Pattern:

1. **Validation Error Test:**

```csharp
[Fact]
public async Task CreatePatient_WithInvalidEmail_ReturnsValidationError()
{
    var result = await _handler.Handle(new CreatePatientCommand
    {
        Email = "invalid"
    });

    result.IsFailure.Should().BeTrue();
    result.HasValidationErrors.Should().BeTrue();
    result.ValidationErrors.Should().ContainKey("email");
}
```

2. **Business Logic Error Test:**

```csharp
[Fact]
public async Task DispenseMedicine_InsufficientStock_ReturnsBusinessError()
{
    var result = await _handler.Handle(new DispenseMedicineCommand
    {
        Quantity = 100
    });

    result.IsFailure.Should().BeTrue();
    result.HasBusinessError.Should().BeTrue();
    result.Code.Should().Be(MessageCodes.Business.INSUFFICIENT_STOCK);
    result.Metadata.Should().NotBeNull();
}
```

---

## Rollback Plan

If migration causes issues, we can temporarily keep both approaches:

1. Keep old `MessageCodes.Old.cs` file
2. Add `#pragma warning disable CS0618` to suppress obsolete warnings
3. Gradually migrate file by file
4. Remove old codes when migration complete

---

## Benefits After Migration

### Before:

- ❌ 200+ message codes
- ❌ Frontend handles 200+ codes
- ❌ Large translation files
- ❌ Maintenance burden

### After:

- ✅ ~30 message codes
- ✅ Frontend handles validation generically
- ✅ Small translation files
- ✅ Easy to maintain
- ✅ Clear separation: validation vs business logic

---

## Timeline

### Recommended Approach: Gradual Migration

**Week 1:**

- ✅ Update Result class (Done)
- ✅ Create new MessageCodes (Done)
- ⏳ Update domain entities (In Progress)

**Week 2:**

- Update validators
- Update handlers

**Week 3:**

- Update controllers
- Update tests

**Week 4:**

- Remove old MessageCodes
- Final testing

---

## Questions?

**Q: Do we need to update everything at once?**
A: No! The old methods are marked `[Obsolete]` but still work. Migrate gradually.

**Q: What about existing frontend code?**
A: Frontend can handle both old and new response formats during migration.

**Q: Can we add new business logic codes?**
A: Yes! Add to `MessageCodes.Business` when you have a business rule that needs specific UI handling.

**Q: When should I use a code vs no code?**
A: Use codes for business logic errors that need specific UI treatment. Don't use codes for validation errors.

---

## Status

- ✅ Result class updated
- ✅ New MessageCodes created
- ⏳ Domain entities migration (in progress)
- ⏳ Validators migration (pending)
- ⏳ Handlers migration (pending)
- ⏳ Controllers migration (pending)
