# IClock Implementation Summary

## Overview

Implemented IClock abstraction for testable time handling and removed direct DateTime.UtcNow usage from domain entities.

**Date:** 2026-02-24

---

## Changes Made

### 1. Created IClock Interface

**File:** `ClinicManagement.Domain/Common/IClock.cs`

```csharp
public interface IClock
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
    DateOnly Today { get; }
}
```

### 2. Implemented SystemClock

**File:** `ClinicManagement.Infrastructure/Services/SystemClock.cs`

```csharp
public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
```

### 3. Registered in DI Container

```csharp
services.AddSingleton<IClock, SystemClock>();
```

---

## Entities Updated

### Entities with CreatedAt Removed Default

1. **StaffBranch** - Added `SetCreatedAt(DateTime)` method
2. **Notification** - Added `SetCreatedAt(DateTime)` method
3. **ClinicUsageMetrics** - Added `SetCreatedAt(DateTime)` method
4. **EmailQueue** - Added `SetCreatedAt(DateTime)` method
5. **SubscriptionPayment** - Added `SetCreatedAt(DateTime)` method
6. **ClinicSubscription** - Added `SetCreatedAt(DateTime)` method
7. **DoctorSpecialization** - Added `SetCreatedAt(DateTime)` method
8. **DoctorProfile** - Removed default from CreatedAt
9. **MedicalVisit** - Removed default from CreatedAt
10. **SubscriptionPlan** - Removed default from EffectiveDate

### Entities with Time-Dependent Logic Updated

#### Patient

**Before:**

```csharp
public int Age => DateTime.UtcNow.Year - DateOfBirth.Year -
    (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
public bool IsAdult => Age >= 18;
public bool IsMinor => Age < 18;
public bool IsSenior => Age >= 65;
```

**After:**

```csharp
public int GetAge(DateTime now) => now.Year - DateOfBirth.Year -
    (now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
public bool IsAdult(DateTime now) => GetAge(now) >= 18;
public bool IsMinor(DateTime now) => GetAge(now) < 18;
public bool IsSenior(DateTime now) => GetAge(now) >= 65;
```

#### Medicine

**Before:**

```csharp
public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.UtcNow.Date;
public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value.Date <= DateTime.UtcNow.Date.AddDays(30);
public StockStatus StockStatus { get; }
public int? DaysUntilExpiry => ExpiryDate?.Subtract(DateTime.UtcNow.Date).Days;
```

**After:**

```csharp
public bool IsExpired(DateTime now) => ExpiryDate.HasValue && ExpiryDate.Value.Date < now.Date;
public bool IsExpiringSoon(DateTime now) => ExpiryDate.HasValue && ExpiryDate.Value.Date <= now.Date.AddDays(30);
public StockStatus GetStockStatus() { ... }
public int? GetDaysUntilExpiry(DateTime now) => ExpiryDate?.Subtract(now.Date).Days;
```

#### UserToken

**Before:**

```csharp
public bool IsExpired => DateTime.UtcNow > ExpiresAt;
public bool IsValid => !IsUsed && !IsExpired;
```

**After:**

```csharp
public bool IsExpired(DateTime now) => now > ExpiresAt;
public bool IsValid(DateTime now) => !IsUsed && !IsExpired(now);
```

#### StaffInvitation

**Before:**

```csharp
public static StaffInvitation Create(
    Guid clinicId,
    string email,
    string role,
    Guid createdByUserId,
    Guid? specializationId = null,
    int expirationDays = DefaultExpirationDays)
{
    var now = DateTime.UtcNow;
    // ...
}
```

**After:**

```csharp
public static StaffInvitation Create(
    Guid clinicId,
    string email,
    string role,
    Guid createdByUserId,
    DateTime now,
    Guid? specializationId = null,
    int expirationDays = DefaultExpirationDays)
{
    // ...
}
```

---

## Benefits

### 1. Testability

- Can inject fake/mock clock for testing
- Test time-dependent logic without waiting
- Test edge cases (midnight, leap years, etc.)

### 2. Consistency

- Single source of truth for current time
- All time operations go through IClock
- Easier to ensure UTC usage

### 3. Domain Purity

- Domain entities don't depend on system clock
- Entities receive time from outside (dependency injection)
- Better separation of concerns

### 4. Flexibility

- Can implement different clock strategies
- Can freeze time for debugging
- Can simulate time zones

---

## Usage Examples

### In Application Layer (Handlers)

```csharp
public class CreatePatientHandler
{
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePatientHandler(IClock clock, IUnitOfWork unitOfWork)
    {
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreatePatientCommand command)
    {
        var patient = new Patient
        {
            // ... properties
            CreatedAt = _clock.UtcNow
        };

        var age = patient.GetAge(_clock.UtcNow);

        await _unitOfWork.Patients.AddAsync(patient);
        return Result.Success();
    }
}
```

### In Tests

```csharp
public class FakeClock : IClock
{
    private DateTime _fixedTime;

    public FakeClock(DateTime fixedTime)
    {
        _fixedTime = fixedTime;
    }

    public DateTime UtcNow => _fixedTime;
    public DateTime Now => _fixedTime.ToLocalTime();
    public DateOnly Today => DateOnly.FromDateTime(_fixedTime.Date);

    public void SetTime(DateTime time) => _fixedTime = time;
}

[Fact]
public void Patient_Age_Should_Be_Calculated_Correctly()
{
    // Arrange
    var clock = new FakeClock(new DateTime(2026, 2, 24));
    var patient = new Patient
    {
        DateOfBirth = new DateTime(2000, 1, 1)
    };

    // Act
    var age = patient.GetAge(clock.UtcNow);

    // Assert
    age.Should().Be(26);
}
```

---

## Migration Guide

### For Existing Code

1. **Inject IClock** in constructors:

```csharp
private readonly IClock _clock;

public MyService(IClock clock)
{
    _clock = clock;
}
```

2. **Replace DateTime.UtcNow** with `_clock.UtcNow`:

```csharp
// Before
var now = DateTime.UtcNow;

// After
var now = _clock.UtcNow;
```

3. **Pass DateTime to entity methods**:

```csharp
// Before
if (patient.IsAdult)

// After
if (patient.IsAdult(_clock.UtcNow))
```

4. **Set CreatedAt explicitly**:

```csharp
// Before
var entity = new MyEntity(); // CreatedAt set automatically

// After
var entity = new MyEntity();
entity.SetCreatedAt(_clock.UtcNow);
```

---

## Files Changed

### New Files: 2

- `ClinicManagement.Domain/Common/IClock.cs`
- `ClinicManagement.Infrastructure/Services/SystemClock.cs`

### Modified Files: 12

- `StaffBranch.cs`
- `Notification.cs`
- `ClinicUsageMetrics.cs`
- `EmailQueue.cs`
- `SubscriptionPayment.cs`
- `ClinicSubscription.cs`
- `DoctorSpecialization.cs`
- `DoctorProfile.cs`
- `MedicalVisit.cs`
- `SubscriptionPlan.cs`
- `Patient.cs`
- `Medicine.cs`
- `UserToken.cs`
- `StaffInvitation.cs`
- `DependencyInjection.cs`

**Total: 16 files**

---

## Next Steps

1. Update all handlers to inject and use IClock
2. Update background jobs to use IClock
3. Update seeders to use IClock
4. Create unit tests using FakeClock
5. Update integration tests to use FakeClock

---

## Status: ✅ COMPLETE

IClock abstraction implemented and all domain entities updated to accept DateTime from outside.

**Completed:** 2026-02-24  
**Commit:** `63a8acf feat: implement IClock abstraction for testable time handling`
