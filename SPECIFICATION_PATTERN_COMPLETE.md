# Specification Pattern Implementation - Complete ✅

## Overview

Successfully implemented the Specification Pattern to encapsulate query logic in a reusable, composable, and testable way. This follows Domain-Driven Design best practices and improves code maintainability.

---

## What is the Specification Pattern?

The Specification Pattern encapsulates business rules and query logic into reusable objects that can be combined and tested independently.

### Before (Query Logic Scattered):

```csharp
// ❌ Query logic in handler - not reusable
var seniorPatientsWithDiabetes = await _context.Patients
    .Where(p => p.DateOfBirth <= DateTime.UtcNow.AddYears(-65))
    .Where(p => p.ChronicDiseases.Any(cd => cd.ChronicDiseaseId == diabetesId))
    .ToListAsync();

// ❌ Same logic duplicated elsewhere
var seniorCount = await _context.Patients
    .Where(p => p.DateOfBirth <= DateTime.UtcNow.AddYears(-65))
    .CountAsync();
```

### After (Reusable Specifications):

```csharp
// ✅ Reusable, testable, composable
var spec = new SeniorPatientSpecification()
    .And(new PatientWithSpecificDiseaseSpecification(diabetesId));

var patients = await _repository.FindAsync(spec);
var count = await _repository.CountAsync(spec);
```

---

## Implementation

### 1. Base Specification Class

**File:** `src/ClinicManagement.Domain/Common/Specification.cs`

```csharp
public abstract class Specification<T>
{
    // Convert to LINQ expression
    public abstract Expression<Func<T, bool>> ToExpression();

    // Check if entity satisfies specification
    public bool IsSatisfiedBy(T entity);

    // Combine specifications
    public Specification<T> And(Specification<T> other);
    public Specification<T> Or(Specification<T> other);
    public Specification<T> Not();
}
```

**Features:**

- ✅ Abstract base class for all specifications
- ✅ Converts to LINQ Expression for database queries
- ✅ Can test in-memory with `IsSatisfiedBy()`
- ✅ Composable with And/Or/Not operators
- ✅ Implicit conversion to Expression

---

### 2. Patient Specifications

**File:** `src/ClinicManagement.Domain/Specifications/PatientSpecifications.cs`

**Created Specifications:**

1. **PatientWithChronicDiseaseSpecification**
   - Finds patients with any chronic disease
2. **SeniorPatientSpecification**
   - Finds patients aged 65 or older
3. **MinorPatientSpecification**
   - Finds patients under 18 years old
4. **PatientNeedingFollowUpSpecification**
   - Finds patients who haven't visited in X months
5. **PatientWithSpecificDiseaseSpecification**
   - Finds patients with a specific chronic disease
6. **PatientByGenderSpecification**
   - Finds patients by gender
7. **PatientInAgeRangeSpecification**
   - Finds patients in specific age range
8. **PatientWithAllergiesSpecification**
   - Finds patients with any allergies
9. **PatientWithSpecificAllergySpecification**
   - Finds patients with specific allergy

---

### 3. Appointment Specifications

**File:** `src/ClinicManagement.Domain/Specifications/AppointmentSpecifications.cs`

**Created Specifications:**

1. **AppointmentOnDateSpecification**
   - Finds appointments on specific date
2. **AppointmentInDateRangeSpecification**
   - Finds appointments in date range
3. **AppointmentByStatusSpecification**
   - Finds appointments by status
4. **PendingAppointmentSpecification**
   - Finds pending appointments
5. **ConfirmedAppointmentSpecification**
   - Finds confirmed appointments
6. **AppointmentByDoctorSpecification**
   - Finds appointments for specific doctor
7. **AppointmentByPatientSpecification**
   - Finds appointments for specific patient
8. **AppointmentByClinicBranchSpecification**
   - Finds appointments at specific branch
9. **TodayAppointmentSpecification**
   - Finds today's appointments
10. **UpcomingAppointmentSpecification**
    - Finds future appointments
11. **PastAppointmentSpecification**
    - Finds past appointments
12. **UnpaidAppointmentSpecification**
    - Finds appointments with unpaid consultation fees

---

### 4. Repository Support

**File:** `src/ClinicManagement.Infrastructure/Data/Repositories/BaseRepository.cs`

**Added Methods:**

```csharp
// Find entities matching specification
Task<IEnumerable<T>> FindAsync(Specification<T> specification);

// Find with pagination
Task<PagedResult<T>> FindPagedAsync(Specification<T> specification, PaginationRequest request);

// Count matching entities
Task<int> CountAsync(Specification<T> specification);

// Check if any match
Task<bool> AnyAsync(Specification<T> specification);
```

---

## Usage Examples

### Example 1: Simple Specification

```csharp
// Find all senior patients
var spec = new SeniorPatientSpecification();
var seniorPatients = await _patientRepository.FindAsync(spec);
```

### Example 2: Combined Specifications (AND)

```csharp
// Find senior patients with chronic diseases
var spec = new SeniorPatientSpecification()
    .And(new PatientWithChronicDiseaseSpecification());

var patients = await _patientRepository.FindAsync(spec);
```

### Example 3: Complex Combination

```csharp
// Find senior female patients with diabetes who need follow-up
var spec = new SeniorPatientSpecification()
    .And(new PatientByGenderSpecification(Gender.Female))
    .And(new PatientWithSpecificDiseaseSpecification(diabetesId))
    .And(new PatientNeedingFollowUpSpecification(6));

var patients = await _patientRepository.FindAsync(spec);
```

### Example 4: OR Logic

```csharp
// Find patients who are either seniors OR have chronic diseases
var spec = new SeniorPatientSpecification()
    .Or(new PatientWithChronicDiseaseSpecification());

var patients = await _patientRepository.FindAsync(spec);
```

### Example 5: NOT Logic

```csharp
// Find patients who are NOT seniors
var spec = new SeniorPatientSpecification().Not();
var nonSeniorPatients = await _patientRepository.FindAsync(spec);
```

### Example 6: With Pagination

```csharp
// Find today's confirmed appointments with pagination
var spec = new TodayAppointmentSpecification()
    .And(new ConfirmedAppointmentSpecification());

var request = new PaginationRequest { PageNumber = 1, PageSize = 20 };
var pagedResult = await _appointmentRepository.FindPagedAsync(spec, request);
```

### Example 7: Count

```csharp
// Count unpaid appointments
var spec = new UnpaidAppointmentSpecification();
var count = await _appointmentRepository.CountAsync(spec);
```

### Example 8: Check Existence

```csharp
// Check if doctor has any appointments today
var spec = new TodayAppointmentSpecification()
    .And(new AppointmentByDoctorSpecification(doctorId));

var hasAppointments = await _appointmentRepository.AnyAsync(spec);
```

### Example 9: In-Memory Testing

```csharp
// Test specification without database
var patient = new Patient { DateOfBirth = DateTime.UtcNow.AddYears(-70) };
var spec = new SeniorPatientSpecification();

var isSenior = spec.IsSatisfiedBy(patient);  // true
```

### Example 10: Use in Handler

```csharp
public class GetSeniorPatientsWithDiabetesQueryHandler
    : IRequestHandler<GetSeniorPatientsWithDiabetesQuery, Result<List<PatientDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<List<PatientDto>>> Handle(
        GetSeniorPatientsWithDiabetesQuery request,
        CancellationToken cancellationToken)
    {
        // Build specification
        var spec = new SeniorPatientSpecification()
            .And(new PatientWithSpecificDiseaseSpecification(request.DiabetesId));

        // Use specification
        var patients = await _unitOfWork.Patients.FindAsync(spec, cancellationToken);

        // Map to DTOs
        var dtos = patients.Select(p => new PatientDto { ... }).ToList();

        return Result<List<PatientDto>>.Ok(dtos);
    }
}
```

---

## Benefits

### 1. Reusability ✅

```csharp
// Use same specification in multiple places
var spec = new SeniorPatientSpecification();

// In handler 1
var patients = await _repository.FindAsync(spec);

// In handler 2
var count = await _repository.CountAsync(spec);

// In handler 3
var exists = await _repository.AnyAsync(spec);
```

### 2. Testability ✅

```csharp
[Fact]
public void SeniorPatientSpecification_ShouldReturnTrue_ForAge65OrOlder()
{
    // Arrange
    var patient = new Patient { DateOfBirth = DateTime.UtcNow.AddYears(-70) };
    var spec = new SeniorPatientSpecification();

    // Act
    var result = spec.IsSatisfiedBy(patient);

    // Assert
    result.Should().BeTrue();
}
```

### 3. Composability ✅

```csharp
// Build complex queries from simple specifications
var spec = specA.And(specB).Or(specC).Not();
```

### 4. Readability ✅

```csharp
// ❌ Before: What does this query do?
var patients = await _context.Patients
    .Where(p => p.DateOfBirth <= DateTime.UtcNow.AddYears(-65))
    .Where(p => p.ChronicDiseases.Any())
    .ToListAsync();

// ✅ After: Clear intent
var spec = new SeniorPatientSpecification()
    .And(new PatientWithChronicDiseaseSpecification());
var patients = await _repository.FindAsync(spec);
```

### 5. Type Safety ✅

```csharp
// Compile-time checking
var spec = new SeniorPatientSpecification();  // Type: Specification<Patient>
var appointments = await _appointmentRepository.FindAsync(spec);  // ❌ Compile error!
```

### 6. Single Responsibility ✅

Each specification has one job - encapsulate one business rule.

### 7. Open/Closed Principle ✅

Add new specifications without modifying existing code.

---

## Real-World Use Cases

### Use Case 1: Patient Risk Assessment

```csharp
// Find high-risk patients (seniors with chronic diseases needing follow-up)
var highRiskSpec = new SeniorPatientSpecification()
    .And(new PatientWithChronicDiseaseSpecification())
    .And(new PatientNeedingFollowUpSpecification(3));

var highRiskPatients = await _repository.FindAsync(highRiskSpec);

// Send reminders
foreach (var patient in highRiskPatients)
{
    await _emailService.SendFollowUpReminderAsync(patient);
}
```

### Use Case 2: Appointment Dashboard

```csharp
// Today's confirmed appointments for specific doctor
var spec = new TodayAppointmentSpecification()
    .And(new ConfirmedAppointmentSpecification())
    .And(new AppointmentByDoctorSpecification(doctorId));

var appointments = await _repository.FindAsync(spec);
```

### Use Case 3: Billing Report

```csharp
// Unpaid appointments in date range
var spec = new UnpaidAppointmentSpecification()
    .And(new AppointmentInDateRangeSpecification(startDate, endDate));

var unpaidAppointments = await _repository.FindAsync(spec);
var totalUnpaid = unpaidAppointments.Sum(a => a.ConsultationFee);
```

### Use Case 4: Patient Outreach

```csharp
// Female patients aged 40-50 for breast cancer screening campaign
var spec = new PatientByGenderSpecification(Gender.Female)
    .And(new PatientInAgeRangeSpecification(40, 50));

var patients = await _repository.FindAsync(spec);
```

### Use Case 5: Allergy Alerts

```csharp
// Patients allergic to penicillin
var spec = new PatientWithSpecificAllergySpecification("Penicillin");
var patients = await _repository.FindAsync(spec);

// Show warning when prescribing penicillin-based drugs
```

---

## Testing Specifications

### Unit Test Example

```csharp
public class SeniorPatientSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_ShouldReturnTrue_ForAge65()
    {
        // Arrange
        var patient = new Patient
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-65)
        };
        var spec = new SeniorPatientSpecification();

        // Act
        var result = spec.IsSatisfiedBy(patient);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_ShouldReturnFalse_ForAge64()
    {
        // Arrange
        var patient = new Patient
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-64)
        };
        var spec = new SeniorPatientSpecification();

        // Act
        var result = spec.IsSatisfiedBy(patient);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void And_ShouldCombineSpecifications()
    {
        // Arrange
        var patient = new Patient
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-70),
            ChronicDiseases = new List<PatientChronicDisease>
            {
                new() { ChronicDiseaseId = Guid.NewGuid() }
            }
        };

        var spec = new SeniorPatientSpecification()
            .And(new PatientWithChronicDiseaseSpecification());

        // Act
        var result = spec.IsSatisfiedBy(patient);

        // Assert
        result.Should().BeTrue();
    }
}
```

---

## Performance Considerations

### ✅ Efficient Database Queries

Specifications compile to SQL - no performance penalty:

```csharp
var spec = new SeniorPatientSpecification();
var patients = await _repository.FindAsync(spec);

// Generates efficient SQL:
// SELECT * FROM Patients WHERE DateOfBirth <= '1959-01-01'
```

### ✅ Query Composition

Multiple specifications combine into single query:

```csharp
var spec = specA.And(specB).And(specC);
// Single database query, not three!
```

### ⚠️ Avoid N+1 Queries

Include related entities when needed:

```csharp
// In repository
var query = _dbSet
    .Include(p => p.ChronicDiseases)
    .Include(p => p.Allergies)
    .Where(specification.ToExpression());
```

---

## Best Practices

### 1. Keep Specifications Simple

Each specification should encapsulate ONE business rule:

```csharp
// ✅ Good: Single responsibility
public class SeniorPatientSpecification : Specification<Patient>

// ❌ Bad: Multiple responsibilities
public class SeniorPatientWithDiabetesAndAllergiesSpecification : Specification<Patient>
```

### 2. Compose Complex Queries

Build complex queries by combining simple specifications:

```csharp
// ✅ Good: Composable
var spec = new SeniorPatientSpecification()
    .And(new PatientWithSpecificDiseaseSpecification(diabetesId))
    .And(new PatientWithAllergiesSpecification());
```

### 3. Name Specifications Clearly

Use descriptive names that express business intent:

```csharp
// ✅ Good names
SeniorPatientSpecification
PatientNeedingFollowUpSpecification
UnpaidAppointmentSpecification

// ❌ Bad names
PatientSpec1
QuerySpecification
FilterSpec
```

### 4. Test Specifications

Write unit tests for each specification:

```csharp
[Fact]
public void Specification_ShouldWork_ForExpectedCase()
{
    // Test with IsSatisfiedBy() - no database needed
}
```

### 5. Use in Handlers

Keep handlers clean by using specifications:

```csharp
// ✅ Good: Clean handler
var spec = new SeniorPatientSpecification();
var patients = await _repository.FindAsync(spec);

// ❌ Bad: Query logic in handler
var patients = await _context.Patients
    .Where(p => p.DateOfBirth <= DateTime.UtcNow.AddYears(-65))
    .ToListAsync();
```

---

## Future Enhancements

### 1. More Specifications

Create specifications for other entities:

- Invoice specifications
- Medicine specifications
- MedicalVisit specifications

### 2. Specification Builder

Fluent API for building specifications:

```csharp
var spec = SpecificationBuilder<Patient>
    .Where(p => p.Age >= 65)
    .And(p => p.ChronicDiseases.Any())
    .Build();
```

### 3. Caching

Cache frequently used specifications:

```csharp
public static class CommonSpecifications
{
    public static readonly SeniorPatientSpecification SeniorPatients = new();
    public static readonly MinorPatientSpecification MinorPatients = new();
}
```

---

## Conclusion

The Specification Pattern implementation is complete and provides:

✅ **18 reusable specifications** (9 for Patient, 12 for Appointment)
✅ **Repository support** for all specification operations
✅ **Composable queries** with And/Or/Not operators
✅ **Testable** without database
✅ **Type-safe** compile-time checking
✅ **Readable** code with clear business intent
✅ **Maintainable** - single responsibility per specification

The pattern is ready for use across the application and can be extended with more specifications as needed.

**Status:** Specification Pattern Complete! ✅

**Next Step:** Outbox Pattern for reliable event publishing! 🚀
