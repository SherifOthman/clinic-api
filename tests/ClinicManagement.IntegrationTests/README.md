# ClinicManagement.IntegrationTests

Integration tests for the Clinic Management API using WebApplicationFactory.

## Test Organization

```
ClinicManagement.IntegrationTests/
├── Auth/                    # Authentication flow tests
│   ├── RegisterTests.cs     # User registration
│   └── LoginTests.cs        # Login and token generation
├── Patients/                # Patient management tests
│   ├── PatientCrudTests.cs  # CRUD operations
│   └── MultiTenancyTests.cs # Multi-tenancy isolation
├── Appointments/            # Appointment workflow tests
│   └── AppointmentWorkflowTests.cs
├── Invoices/                # Invoice and payment tests
│   └── InvoicePaymentFlowTests.cs
├── Helpers/                 # Test utilities
│   ├── TestAuthHelper.cs    # Authentication helpers
│   └── TestDataBuilder.cs   # Test data creation
└── TestWebApplicationFactory.cs  # Test server setup
```

## What Integration Tests Cover

### 1. Authentication & Authorization

- User registration with validation
- Login and token generation
- Refresh token cookies
- Unauthorized access prevention

### 2. Patient Management (CRUD)

- Create patient with validation
- Get patients (paginated)
- Get patient by ID
- Update patient information
- Delete patient (soft delete)
- Unauthorized access handling

### 3. Multi-Tenancy Isolation

- Patients isolated by clinic
- Cannot access other clinic's data
- Cannot modify other clinic's data
- Cannot delete other clinic's data

### 4. Appointment Workflow

- Create appointment
- Confirm appointment (Pending → Confirmed)
- Complete appointment (Confirmed → Completed)
- Cancel appointment
- Queue number generation
- Conflict detection

### 5. Invoice & Payment Flow

- Create invoice with items
- Record partial payment
- Record full payment
- Payment validation (cannot exceed invoice amount)
- Invoice status transitions (Draft → Issued → PartiallyPaid → FullyPaid)
- Cancel invoice
- Get invoice payments

## Running Integration Tests

```bash
# Run all integration tests
dotnet test ClinicManagement.IntegrationTests

# Run specific test class
dotnet test --filter "FullyQualifiedName~RegisterTests"

# Run tests in a specific namespace
dotnet test --filter "FullyQualifiedName~Auth"

# Run with detailed output
dotnet test ClinicManagement.IntegrationTests --verbosity detailed
```

## Test Infrastructure

### TestWebApplicationFactory

- Configures in-memory database for each test
- Overrides services for testing
- Ensures database is created before tests run
- Uses unique database name per test run to avoid conflicts

### TestAuthHelper

- Simplifies authentication in tests
- Provides `RegisterAndLoginAsync` for quick setup
- Handles token management
- Sets authorization headers

### TestDataBuilder

- Creates test data (clinics, patients, doctors, etc.)
- Provides consistent test data
- Handles entity relationships
- Simplifies test setup

## Test Patterns

### Arrange-Act-Assert (AAA)

All tests follow the AAA pattern:

```csharp
[Fact]
public async Task CreatePatient_WithValidData_ShouldReturnCreated()
{
    // Arrange
    var token = await TestAuthHelper.GetAuthTokenAsync(_client);
    TestAuthHelper.SetAuthToken(_client, token);
    var request = new { /* ... */ };

    // Act
    var response = await _client.PostAsJsonAsync("/api/patients", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### Test Isolation

- Each test uses a fresh in-memory database
- Tests don't depend on each other
- Tests can run in parallel
- No shared state between tests

### Authentication Setup

Most tests require authentication:

```csharp
var token = await TestAuthHelper.GetAuthTokenAsync(_client);
TestAuthHelper.SetAuthToken(_client, token);
await CompleteOnboardingAsync(); // If needed
```

## Key Testing Scenarios

### 1. Happy Path

Tests verify that valid requests work correctly:

- Valid data returns expected results
- Status codes are correct
- Response data is accurate

### 2. Validation

Tests verify that invalid requests are rejected:

- Invalid email formats
- Weak passwords
- Missing required fields
- Invalid data types

### 3. Business Rules

Tests verify business logic:

- Appointment state machine
- Invoice payment rules
- Stock management
- Multi-tenancy isolation

### 4. Security

Tests verify security measures:

- Unauthorized access is blocked
- Multi-tenancy isolation works
- Cannot access other clinic's data
- Authentication is required

## Best Practices

1. **Use unique data**: Generate unique emails, names, etc. using `Guid.NewGuid()`
2. **Test one thing**: Each test should verify a single behavior
3. **Clean assertions**: Use FluentAssertions for readable assertions
4. **Descriptive names**: Test names should clearly describe what is being tested
5. **Minimal setup**: Only set up what's needed for the test
6. **Test real scenarios**: Integration tests should test real user workflows

## Common Issues

### Database Conflicts

If tests fail due to database conflicts, ensure each test uses a unique database:

- TestWebApplicationFactory creates unique database per instance
- Don't share HttpClient instances between unrelated tests

### Authentication Failures

If authentication fails:

- Ensure user registration completes successfully
- Check that onboarding is completed if required
- Verify token is set in authorization header

### 404 Not Found

If getting 404 errors:

- Ensure onboarding is completed (creates clinic and branch)
- Verify entity IDs are correct
- Check multi-tenancy isolation (accessing wrong clinic's data)

## Continuous Integration

Integration tests are run:

- On every commit
- On pull requests
- Before deployment

All integration tests must pass before code can be merged.

## Performance

Integration tests are slower than unit tests because they:

- Start a test server
- Use a database (even in-memory)
- Make HTTP requests
- Serialize/deserialize JSON

Expected run time: 10-30 seconds for full suite.
