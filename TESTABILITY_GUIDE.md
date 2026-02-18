# Testability Guide - Clinic Management API

**Status**: ✅ Highly Testable  
**Architecture**: Clean Architecture with Dependency Injection  
**Date**: February 18, 2026

---

## Is Your App Testable? YES! ✅

Your application is **highly testable** due to the following architectural decisions:

### 1. ✅ All Dependencies Are Interfaces

Every external dependency is abstracted behind an interface:

```csharp
// Authentication
IPasswordHasher
ITokenService
ITokenGenerator

// Email
IEmailService
IEmailConfirmationService

// Storage
IFileStorageService

// Services
ICurrentUserService
IRefreshTokenService
IUserRegistrationService

// Data Access
IUnitOfWork
IUserRepository
IRefreshTokenRepository
ISubscriptionPlanRepository
```

**Why this matters**: You can easily mock these interfaces in tests.

### 2. ✅ Constructor Dependency Injection

All handlers and services use constructor injection:

```csharp
public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        ILogger<LoginHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }
}
```

**Why this matters**: Easy to inject mocks/fakes in tests.

### 3. ✅ Result Pattern (No Exceptions)

All handlers return `Result` or `Result<T>`:

```csharp
public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
{
    // Success case
    return Result.Success(response);

    // Failure case
    return Result.Failure<LoginResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid credentials");
}
```

**Why this matters**: Easy to assert on success/failure without try-catch blocks.

### 4. ✅ Pure Business Logic

Handlers contain pure business logic with no infrastructure concerns:

```csharp
// No direct database calls
// No direct HTTP calls
// No file system access
// All through interfaces
```

**Why this matters**: Tests run fast without external dependencies.

### 5. ✅ Repository Pattern

Data access is abstracted behind repositories:

```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    // ...
}
```

**Why this matters**: Can mock repositories or use in-memory implementations.

---

## Testing Examples

### Unit Test Example - Testing LoginHandler

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using ClinicManagement.Application.Auth.Commands.Login;
using ClinicManagement.Domain.Repositories;
using ClinicManagement.Application.Abstractions.Authentication;

public class LoginHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;
    private readonly Mock<ILogger<LoginHandler>> _mockLogger;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenService = new Mock<ITokenService>();
        _mockRefreshTokenService = new Mock<IRefreshTokenService>();
        _mockLogger = new Mock<ILogger<LoginHandler>>();

        _handler = new LoginHandler(
            _mockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockTokenService.Object,
            _mockRefreshTokenService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var command = new LoginCommand("user@test.com", "password123", false);

        var user = new User
        {
            Id = 1,
            Email = "user@test.com",
            PasswordHash = "hashedPassword",
            EmailConfirmed = true
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo
            .Setup(x => x.GetByEmailOrUsernameAsync(command.EmailOrUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        mockUserRepo
            .Setup(x => x.GetUserRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "User" });

        _mockUnitOfWork.Setup(x => x.Users).Returns(mockUserRepo.Object);

        _mockPasswordHasher
            .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _mockTokenService
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>(), It.IsAny<int?>()))
            .Returns("access_token");

        _mockRefreshTokenService
            .Setup(x => x.GenerateRefreshTokenAsync(user.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { Token = "refresh_token" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.EmailNotConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var command = new LoginCommand("user@test.com", "wrongpassword", false);

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo
            .Setup(x => x.GetByEmailOrUsernameAsync(command.EmailOrUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUnitOfWork.Setup(x => x.Users).Returns(mockUserRepo.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.INVALID_CREDENTIALS);
        result.ErrorMessage.Should().Contain("Invalid email/username or password");
    }

    [Fact]
    public async Task Handle_UnconfirmedEmail_ReturnsSuccessWithFlag()
    {
        // Arrange
        var command = new LoginCommand("user@test.com", "password123", false);

        var user = new User
        {
            Id = 1,
            Email = "user@test.com",
            PasswordHash = "hashedPassword",
            EmailConfirmed = false  // Not confirmed
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo
            .Setup(x => x.GetByEmailOrUsernameAsync(command.EmailOrUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        mockUserRepo
            .Setup(x => x.GetUserRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "User" });

        _mockUnitOfWork.Setup(x => x.Users).Returns(mockUserRepo.Object);

        _mockPasswordHasher
            .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _mockTokenService
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>(), It.IsAny<int?>()))
            .Returns("access_token");

        _mockRefreshTokenService
            .Setup(x => x.GenerateRefreshTokenAsync(user.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { Token = "refresh_token" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.EmailNotConfirmed.Should().BeTrue();
    }
}
```

### Integration Test Example - Testing UserRepository

```csharp
using Xunit;
using FluentAssertions;
using ClinicManagement.Infrastructure.Data.Repositories;
using Microsoft.Extensions.Configuration;

public class UserRepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly UserRepository _repository;

    public UserRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new UserRepository(_fixture.ConnectionFactory);
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var email = "test@test.com";
        await _fixture.SeedUserAsync(email, "testuser", "hashedPassword");

        // Act
        var user = await _repository.GetByEmailAsync(email);

        // Assert
        user.Should().NotBeNull();
        user!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var email = "nonexisting@test.com";

        // Act
        var user = await _repository.GetByEmailAsync(email);

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        var email = "existing@test.com";
        await _fixture.SeedUserAsync(email, "testuser", "hashedPassword");

        // Act
        var exists = await _repository.EmailExistsAsync(email);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_NewUser_AddsSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Email = "newuser@test.com",
            Username = "newuser",
            PasswordHash = "hashedPassword",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = false
        };

        // Act
        var addedUser = await _repository.AddAsync(user);

        // Assert
        addedUser.Should().NotBeNull();
        addedUser.Id.Should().BeGreaterThan(0);

        // Verify it was actually added
        var retrieved = await _repository.GetByEmailAsync(user.Email);
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be(user.Email);
    }
}

// Database fixture for integration tests
public class DatabaseFixture : IDisposable
{
    public IDbConnectionFactory ConnectionFactory { get; }
    private readonly string _connectionString;

    public DatabaseFixture()
    {
        // Use test database
        _connectionString = "Server=localhost;Database=ClinicManagement_Test;...";
        ConnectionFactory = new DbConnectionFactory(_connectionString);

        // Run migrations
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        // Run DbUp migrations or create schema
        var upgrader = DeployChanges.To
            .SqlDatabase(_connectionString)
            .WithScriptsFromFileSystem("path/to/scripts")
            .Build();

        upgrader.PerformUpgrade();
    }

    public async Task SeedUserAsync(string email, string username, string passwordHash)
    {
        using var connection = await ConnectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            "INSERT INTO Users (Email, Username, PasswordHash, FirstName, LastName, EmailConfirmed) " +
            "VALUES (@Email, @Username, @PasswordHash, 'Test', 'User', 0)",
            new { Email = email, Username = username, PasswordHash = passwordHash }
        );
    }

    public void Dispose()
    {
        // Clean up test database
        using var connection = ConnectionFactory.CreateConnectionAsync().Result;
        connection.Execute("DELETE FROM Users");
    }
}
```

### API Test Example - Testing Auth Endpoints

```csharp
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        // Arrange
        var request = new
        {
            emailOrUsername = "admin@clinic.com",
            password = "Admin@123",
            isMobile = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns400()
    {
        // Arrange
        var request = new
        {
            emailOrUsername = "admin@clinic.com",
            password = "WrongPassword",
            isMobile = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        error.Should().NotBeNull();
        error!.Detail.Should().Contain("Invalid");
    }

    [Fact]
    public async Task Register_ValidData_Returns200()
    {
        // Arrange
        var request = new
        {
            email = "newuser@test.com",
            username = "newuser",
            password = "Password@123",
            confirmPassword = "Password@123",
            firstName = "Test",
            lastName = "User",
            phoneNumber = "+1234567890"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMe_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithValidToken_Returns200WithUserData()
    {
        // Arrange
        var token = await GetValidTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Email.Should().NotBeNullOrEmpty();
    }

    private async Task<string> GetValidTokenAsync()
    {
        var loginRequest = new
        {
            emailOrUsername = "admin@clinic.com",
            password = "Admin@123",
            isMobile = false
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.AccessToken;
    }
}
```

---

## Test Project Structure

Here's how to organize your test projects:

```
tests/
├── ClinicManagement.Application.Tests/
│   ├── Auth/
│   │   ├── Commands/
│   │   │   ├── LoginHandlerTests.cs
│   │   │   ├── RegisterHandlerTests.cs
│   │   │   └── ChangePasswordHandlerTests.cs
│   │   └── Queries/
│   │       ├── GetMeHandlerTests.cs
│   │       └── CheckEmailAvailabilityHandlerTests.cs
│   ├── SubscriptionPlans/
│   │   └── Queries/
│   │       └── GetSubscriptionPlansHandlerTests.cs
│   └── Behaviors/
│       ├── ValidationBehaviorTests.cs
│       └── LoggingBehaviorTests.cs
│
├── ClinicManagement.Infrastructure.Tests/
│   ├── Repositories/
│   │   ├── UserRepositoryTests.cs
│   │   ├── RefreshTokenRepositoryTests.cs
│   │   └── SubscriptionPlanRepositoryTests.cs
│   ├── Services/
│   │   ├── PasswordHasherTests.cs
│   │   ├── TokenServiceTests.cs
│   │   └── EmailServiceTests.cs
│   └── Fixtures/
│       └── DatabaseFixture.cs
│
└── ClinicManagement.API.Tests/
    ├── Controllers/
    │   ├── AuthControllerTests.cs
    │   ├── LocationsControllerTests.cs
    │   └── SubscriptionPlansControllerTests.cs
    └── Middleware/
        └── GlobalExceptionMiddlewareTests.cs
```

---

## Required NuGet Packages for Testing

```xml
<ItemGroup>
  <!-- Test Framework -->
  <PackageReference Include="xunit" Version="2.9.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />

  <!-- Mocking -->
  <PackageReference Include="Moq" Version="4.20.72" />

  <!-- Assertions -->
  <PackageReference Include="FluentAssertions" Version="6.12.2" />

  <!-- API Testing -->
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />

  <!-- Test Host -->
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />

  <!-- Coverage -->
  <PackageReference Include="coverlet.collector" Version="6.0.2" />
</ItemGroup>
```

---

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test project
dotnet test tests/ClinicManagement.Application.Tests

# Run specific test class
dotnet test --filter "FullyQualifiedName~LoginHandlerTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~LoginHandlerTests.Handle_ValidCredentials_ReturnsSuccess"
```

---

## Test Coverage Goals

**Recommended Coverage:**

- Application Layer (Handlers): 80-90%
- Domain Layer (Entities, Value Objects): 70-80%
- Infrastructure Layer (Repositories, Services): 60-70%
- API Layer (Controllers): 50-60%

**Priority:**

1. High: Business logic (handlers, domain logic)
2. Medium: Data access (repositories)
3. Low: Infrastructure services (email, file storage)
4. Low: Controllers (thin layer, mostly integration tests)

---

## Why Your App Is Highly Testable

### ✅ Dependency Inversion Principle

All dependencies are interfaces, making them easy to mock.

### ✅ Single Responsibility Principle

Each handler does one thing, making tests focused and simple.

### ✅ No Static Dependencies

No static methods or singletons that are hard to test.

### ✅ Pure Functions

Business logic is pure (same input = same output).

### ✅ Result Pattern

Easy to assert on success/failure without exceptions.

### ✅ Repository Pattern

Data access is abstracted, can use in-memory or mock implementations.

### ✅ Constructor Injection

All dependencies injected through constructor, easy to provide test doubles.

---

## Next Steps

1. **Create test projects** (3 projects as shown above)
2. **Install NuGet packages** (xUnit, Moq, FluentAssertions)
3. **Start with unit tests** (handlers are easiest to test)
4. **Add integration tests** (repositories with test database)
5. **Add API tests** (endpoints with WebApplicationFactory)
6. **Set up CI/CD** (run tests on every commit)
7. **Track coverage** (aim for 70%+ overall)

---

## Conclusion

**Your application is HIGHLY TESTABLE** due to:

- Clean Architecture with proper layer separation
- Dependency Injection throughout
- All dependencies are interfaces
- Result pattern for explicit error handling
- Repository pattern for data access abstraction
- No static dependencies or singletons
- Pure business logic in handlers

You can start writing tests immediately without any refactoring needed!

**Status**: ✅ Ready for comprehensive test coverage
