# ClinicManagement.Tests

Comprehensive test suite for the Clinic Management API.

## Test Organization

```
ClinicManagement.Tests/
├── Domain/              # Domain entity tests
│   ├── AppointmentTests.cs
│   ├── InvoiceTests.cs
│   ├── MedicineTests.cs
│   ├── PatientTests.cs
│   └── PaymentTests.cs
├── Services/            # Service layer tests
│   ├── CodeGeneratorServiceTests.cs
│   ├── PhoneValidationServiceTests.cs
│   ├── RefreshTokenServiceTests.cs
│   └── TokenServiceTests.cs
├── Extensions/          # Extension method tests
│   ├── DateTimeExtensionsTests.cs
│   └── QueryExtensionsTests.cs
├── Validation/          # Validation logic tests
│   └── CustomValidatorsTests.cs
└── Helpers/             # Test utilities
    └── TestHttpContextAccessor.cs
```

## Test Coverage

### Domain Entities (56 tests)

- **Appointments** (8 tests): State machine, lifecycle, business rules
- **Patients** (9 tests): Creation, allergies, phone numbers, age calculations
- **Medicines** (13 tests): Inventory, stock management, pricing, expiry
- **Invoices** (12 tests): Creation, items, discounts, status transitions
- **Payments** (14 tests): Payment processing, methods, validation, accumulation

### Services (39 tests)

- **TokenService** (14 tests): JWT generation, validation, claims, expiration
- **RefreshTokenService** (17 tests): Token rotation, revocation, cleanup, isolation
- **PhoneValidationService** (4 tests): International phone validation
- **CodeGeneratorService** (2 tests): Sequential number generation
- **DateTimeExtensions** (4 tests): Age calculation utilities

### Extensions (12 tests)

- **QueryExtensions** (12 tests): Query building, pagination, search, filtering

### Validation (8 tests)

- **CustomValidators** (8 tests): Date validation (past/future/today/null)

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~AppointmentTests"

# Run tests in a specific namespace
dotnet test --filter "FullyQualifiedName~Domain"

# Generate code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Frameworks & Libraries

- **xUnit**: Test framework
- **FluentAssertions**: Readable assertions
- **Moq**: Mocking framework
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for integration tests

## Writing New Tests

### Test Naming Convention

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange

    // Act

    // Assert
}
```

### Example Test

```csharp
[Fact]
public void Create_WithValidData_ShouldCreatePatient()
{
    // Arrange
    var patientCode = "PAT-2026-000001";
    var clinicId = Guid.NewGuid();
    var fullName = "John Doe";
    var dateOfBirth = new DateTime(1990, 1, 1);

    // Act
    var patient = Patient.Create(
        patientCode,
        clinicId,
        fullName,
        Gender.Male,
        dateOfBirth);

    // Assert
    patient.Should().NotBeNull();
    patient.PatientCode.Should().Be(patientCode);
    patient.FullName.Should().Be(fullName);
}
```

## Test Helpers

### TestHttpContextAccessor

Provides a fake `IHttpContextAccessor` for testing services that depend on user context:

```csharp
var httpContextAccessor = new TestHttpContextAccessor(clinicId, userId);
var currentUserService = new CurrentUserService(httpContextAccessor);
```

## Best Practices

1. **Test one thing at a time**: Each test should verify a single behavior
2. **Use descriptive names**: Test names should clearly describe what is being tested
3. **Follow AAA pattern**: Arrange, Act, Assert
4. **Keep tests independent**: Tests should not depend on each other
5. **Use test data builders**: For complex object creation
6. **Test edge cases**: Include boundary conditions and error scenarios
7. **Mock external dependencies**: Use in-memory database or mocks for external services

## Continuous Integration

Tests are automatically run on:

- Every commit
- Pull requests
- Before deployment

All tests must pass before code can be merged.
