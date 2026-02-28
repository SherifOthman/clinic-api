# Test Refactoring Examples: UnitOfWork Mocks → EF Core In-Memory

This document shows concrete before/after examples for all existing test files.

---

## Example 1: ChangePasswordHandlerTests

### BEFORE (Mocking UnitOfWork)

```csharp
public class ChangePasswordHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ILogger<ChangePasswordHandler>> _loggerMock = new();
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        _handler = new ChangePasswordHandler(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock
            .Setup(x => x.GetRequiredClinicId())
            .Returns(userId);

        _unitOfWorkMock
            .Setup(x => x.Users.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new ChangePasswordCommand("old", "new");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenCurrentPasswordIsCorrect()
    {
        // Arrange
        var user = new User { PasswordHash = "oldhash" };

        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(user.Id);

        _unitOfWorkMock
            .Setup(x=>x.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x=>x.VerifyPassword("old", "oldhash"))
            .Returns(true);

        _passwordHasherMock
            .Setup(x=>x.HashPassword("new"))
            .Returns("newhash");

        var command = new ChangePasswordCommand("old", "new");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("newhash");

        _unitOfWorkMock.Verify(
            x=>x.Users.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

### AFTER (EF Core In-Memory)

```csharp
public class ChangePasswordHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ILogger<ChangePasswordHandler>> _loggerMock = new();
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _handler = new ChangePasswordHandler(
            _context,
            _passwordHasherMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(userId);

        // No user in database - test will fail naturally

        var command = new ChangePasswordCommand("old", "new");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenCurrentPasswordIsCorrect()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordHash = "oldhash"
        };

        // Add user to in-memory database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(user.Id);

        _passwordHasherMock
            .Setup(x=>x.VerifyPassword("old", "oldhash"))
            .Returns(true);

        _passwordHasherMock
            .Setup(x=>x.HashPassword("new"))
            .Returns("newhash");

        var command = new ChangePasswordCommand("old", "new");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify in database
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.PasswordHash.Should().Be("newhash");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

**Key Changes:**

- ❌ Remove `Mock<IUnitOfWork>`
- ✅ Add real `ApplicationDbContext` with in-memory database
- ❌ Remove `.Setup(x => x.Users.GetByIdAsync())`
- ✅ Add real data: `_context.Users.Add(user)`
- ❌ Remove `.Verify(x => x.Users.UpdateAsync())`
- ✅ Verify by querying database: `_context.Users.FindAsync()`

---

## Example 2: ConfirmEmailHandlerTests

### BEFORE (Mocking UnitOfWork)

```csharp
public class ConfirmEmailHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmailTokenService> _emailTokenServiceMock = new();
    private readonly Mock<ILogger<ConfirmEmailHandler>> _loggerMock = new();
    private readonly ConfirmEmailHandler _handler;

    public ConfirmEmailHandlerTests()
    {
        _handler = new ConfirmEmailHandler(
            _unitOfWorkMock.Object,
            _emailTokenServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExists()
    {
        var email = "test@test.com";

        _unitOfWorkMock
            .Setup(u => u.Users.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new ConfirmEmailCommand(email, "token");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldConfirmEmail_WhenTokenIsValid()
    {
        var user = new User { Email = "test@test.com" };

        _unitOfWorkMock
            .Setup(x => x.Users.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _emailTokenServiceMock
            .Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _emailTokenServiceMock
            .Setup(x => x.ConfirmEmailAsync(user, "token", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ConfirmEmailCommand(user.Email, "token");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
    }
}
```

### AFTER (EF Core In-Memory)

```csharp
public class ConfirmEmailHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IEmailTokenService> _emailTokenServiceMock = new();
    private readonly Mock<ILogger<ConfirmEmailHandler>> _loggerMock = new();
    private readonly ConfirmEmailHandler _handler;

    public ConfirmEmailHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _handler = new ConfirmEmailHandler(
            _context,
            _emailTokenServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExists()
    {
        var email = "test@test.com";

        // No user in database - test will fail naturally

        var command = new ConfirmEmailCommand(email, "token");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldConfirmEmail_WhenTokenIsValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        // Add user to in-memory database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _emailTokenServiceMock
            .Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _emailTokenServiceMock
            .Setup(x => x.ConfirmEmailAsync(user, "token", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ConfirmEmailCommand(user.Email, "token");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Example 3: DeleteProfileImageHandlerTests

### BEFORE (Mocking UnitOfWork)

```csharp
public class DeleteProfileImageHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<ILogger<DeleteProfileImageHandler>> _loggerMock = new();
    private readonly DeleteProfileImageHandler _handler;

    public DeleteProfileImageHandlerTests()
    {
        _handler = new DeleteProfileImageHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFileAndClearProfileImage_WhenUserHasProfileImage()
    {
        // Arrange
        var user = new User { ProfileImageUrl = "profile.jpg" };

        _currentUserServiceMock
            .Setup(s=>s.GetRequiredUserId())
            .Returns(user.Id);

        _unitOfWorkMock
            .Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new DeleteProfileImageCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.ProfileImageUrl.Should().BeNull();

        _fileStorageServiceMock.Verify(s => s.DeleteFileAsync("profile.jpg"), Times.Once);
        _unitOfWorkMock.Verify(u => u.Users.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### AFTER (EF Core In-Memory)

```csharp
public class DeleteProfileImageHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<ILogger<DeleteProfileImageHandler>> _loggerMock = new();
    private readonly DeleteProfileImageHandler _handler;

    public DeleteProfileImageHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _handler = new DeleteProfileImageHandler(
            _context,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFileAndClearProfileImage_WhenUserHasProfileImage()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            ProfileImageUrl = "profile.jpg"
        };

        // Add user to in-memory database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s=>s.GetRequiredUserId())
            .Returns(user.Id);

        // Act
        var result = await _handler.Handle(new DeleteProfileImageCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify in database
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.ProfileImageUrl.Should().BeNull();

        _fileStorageServiceMock.Verify(s => s.DeleteFileAsync("profile.jpg"), Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Example 4: ForgotPasswordHandlerTests

### BEFORE (Mocking UnitOfWork)

```csharp
public class ForgotPasswordHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailTokenService> _tokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ForgotPasswordHandler>> _loggerMock;
    private readonly ForgotPasswordHandler _handler;

    public ForgotPasswordHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenServiceMock = new Mock<IEmailTokenService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ForgotPasswordHandler>>();

        var smtpOptions = Options.Create(new SmtpOptions { FrontendUrl = "https://example.com" });

        _handler = new ForgotPasswordHandler(
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _emailServiceMock.Object,
            smtpOptions,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSendEmail_WhenUserExists()
    {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "hash123" };

        _unitOfWorkMock
            .Setup(x => x.Users.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(x => x.GeneratePasswordResetToken(user.Id, user.Email, user.PasswordHash))
            .Returns("reset-token-123");

        var command = new ForgotPasswordCommand(user.Email);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _emailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(
                user.Email,
                It.IsAny<string>(),
                It.Is<string>(link => link.Contains("reset-token-123")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

### AFTER (EF Core In-Memory)

```csharp
public class ForgotPasswordHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IEmailTokenService> _tokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ForgotPasswordHandler>> _loggerMock;
    private readonly ForgotPasswordHandler _handler;

    public ForgotPasswordHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _tokenServiceMock = new Mock<IEmailTokenService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ForgotPasswordHandler>>();

        var smtpOptions = Options.Create(new SmtpOptions { FrontendUrl = "https://example.com" });

        _handler = new ForgotPasswordHandler(
            _context,
            _tokenServiceMock.Object,
            _emailServiceMock.Object,
            smtpOptions,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSendEmail_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordHash = "hash123"
        };

        // Add user to in-memory database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _tokenServiceMock
            .Setup(x => x.GeneratePasswordResetToken(user.Id, user.Email, user.PasswordHash))
            .Returns("reset-token-123");

        var command = new ForgotPasswordCommand(user.Email);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _emailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(
                user.Email,
                It.IsAny<string>(),
                It.Is<string>(link => link.Contains("reset-token-123")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Summary of Changes

### What to Remove:

```csharp
private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

_unitOfWorkMock
    .Setup(x => x.Users.GetByIdAsync(...))
    .ReturnsAsync(user);

_unitOfWorkMock
    .Setup(x => x.Users.GetByEmailAsync(...))
    .ReturnsAsync(user);

_unitOfWorkMock.Verify(
    x => x.Users.UpdateAsync(...),
    Times.Once);
```

### What to Add:

```csharp
private readonly ApplicationDbContext _context;

public TestClass()
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    _context = new ApplicationDbContext(options);
}

// In test methods:
_context.Users.Add(user);
await _context.SaveChangesAsync();

// Verify:
var updatedUser = await _context.Users.FindAsync(user.Id);
updatedUser!.Property.Should().Be(expectedValue);

// Cleanup:
public void Dispose()
{
    _context.Dispose();
}
```

### Benefits:

✅ Tests verify actual database behavior (more realistic)
✅ No complex mock setups
✅ Easier to understand and maintain
✅ Tests catch more bugs (e.g., missing SaveChanges calls)
✅ Faster test execution (in-memory is fast)
✅ Less brittle (no mock verification failures)
