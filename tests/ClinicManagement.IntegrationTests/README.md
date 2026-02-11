# Integration Tests

This project contains integration tests for the Clinic Management API.

## Test Coverage

### Authentication Tests (5 tests)

- User registration with valid/invalid data
- Login with valid/invalid credentials
- Refresh token cookie handling

### Patient CRUD Tests (2 tests)

- Create patient without authentication (authorization test)
- Basic patient operations

## Running Tests

```bash
# Run all tests
dotnet test

# Run only integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run specific test class
dotnet test --filter "FullyQualifiedName~LoginTests"
```

## Test Infrastructure

- `TestWebApplicationFactory`: Configures in-memory database and test services
- `TestAuthHelper`: Helper methods for authentication in tests
- `TestConstants`: Shared constants for test data (subscription plans, specializations)

## Notes

- Tests use an in-memory database that is created fresh for each test run
- Reference data (subscription plans, specializations) is seeded automatically
- Each test class uses a separate database instance for isolation
