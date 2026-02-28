# EF Core Migration Guide: From Repository/UnitOfWork to DbContext

## Overview

This guide shows how to migrate from the Repository + UnitOfWork pattern to using EF Core DbContext directly. This eliminates ~3000 lines of unnecessary abstraction code while maintaining testability.

## Current Architecture (To Remove)

```
Handler → IUnitOfWork → IRepository → Dapper SQL
```

**Problems:**

- Unnecessary abstraction layers
- ~3000 lines of boilerplate code
- Manual SQL queries
- Mocking complexity in tests
- No automatic migrations
- No change tracking

## New Architecture (EF Core)

```
Handler → IApplicationDbContext (DbContext) → EF Core → Database
```

**Benefits:**

- Direct DbContext usage (DbContext IS a repository + unit of work)
- Automatic migrations
- Change tracking
- Type safety
- LINQ queries
- Easier testing with in-memory database
- ~3000 fewer lines of code

---

## Part 1: Create IApplicationDbContext Interface

This interface exposes DbSets and allows easy testing/mocking.

```csharp
// src/ClinicManagement.Application/Abstractions/Data/IApplicationDbContext.cs
namespace ClinicManagement.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    // DbSets for all entities
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<StaffInvitation> StaffInvitations { get; }
    DbSet<Staff> Staff { get; }
    DbSet<DoctorProfile> DoctorProfiles { get; }
    DbSet<Specialization> Specializations { get; }
    DbSet<Clinic> Clinics { get; }
    DbSet<ClinicBranch> ClinicBranches { get; }
    DbSet<UserRoleHistory> UserRoleHistory { get; }
    DbSet<ClinicSubscription> ClinicSubscriptions { get; }
    DbSet<ClinicUsageMetrics> ClinicUsageMetrics { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<EmailQueue> EmailQueue { get; }
    DbSet<DoctorSpecialization> DoctorSpecializations { get; }
    DbSet<StaffBranch> StaffBranches { get; }

    // SaveChanges methods
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();

    // Transaction support (if needed)
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
```

---

## Part 2: Implement ApplicationDbContext

```csharp
// src/ClinicManagement.Infrastructure/Persistence/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicBranch> ClinicBranches => Set<ClinicBranch>();
    public DbSet<UserRoleHistory> UserRoleHistory => Set<UserRoleHistory>();
    public DbSet<ClinicSubscription> ClinicSubscriptions => Set<ClinicSubscription>();
    public DbSet<ClinicUsageMetrics> ClinicUsageMetrics => Set<ClinicUsageMetrics>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EmailQueue> EmailQueue => Set<EmailQueue>();
    public DbSet<DoctorSpecialization> DoctorSpecializations => Set<DoctorSpecialization>();
    public DbSet<StaffBranch> StaffBranches => Set<StaffBranch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }
}
```

---

## Part 3: Refactor Handlers

### BEFORE (with UnitOfWork):

```csharp
public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser,
        ILogger<ChangePasswordHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();

        // Using repository
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Invalid current password for user: {UserId}", user.Id);
            return Result.Failure(ErrorCodes.INVALID_CREDENTIALS, "Current password is incorrect");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        // Using repository Update method
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Password changed successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}
```

### AFTER (with DbContext):

```csharp
public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser,
        ILogger<ChangePasswordHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();

        // Direct DbSet access with EF Core
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Invalid current password for user: {UserId}", user.Id);
            return Result.Failure(ErrorCodes.INVALID_CREDENTIALS, "Current password is incorrect");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        // EF Core change tracking - no Update() call needed!
        // Just call SaveChanges
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password changed successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}
```

**Key Changes:**

1. `IUnitOfWork` → `IApplicationDbContext`
2. `_unitOfWork.Users.GetByIdAsync()` → `_context.Users.FirstOrDefaultAsync()`
3. `_unitOfWork.Users.UpdateAsync()` → Just modify entity, EF tracks changes
4. No explicit `Update()` call needed - EF Core change tracking handles it
5. `SaveChangesAsync()` saves all tracked changes

---

## Part 4: Refactor Tests

### BEFORE (Mocking UnitOfWork):

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

        _unitOfWorkMock.Verify(x=>x.Users.UpdateAsync(user, It.IsAny<CancellationToken>())
        ,Times.Once);
    }
}
```

### AFTER (Using In-Memory Database):

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
        // Create in-memory database
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

1. No more `Mock<IUnitOfWork>` - use real `ApplicationDbContext` with in-memory database
2. Use `UseInMemoryDatabase()` with unique database name per test
3. Add test data directly: `_context.Users.Add(user)`
4. Verify results by querying database: `_context.Users.FindAsync()`
5. Implement `IDisposable` to clean up context
6. Tests are now integration tests that verify actual database behavior

---

## Part 5: Common EF Core Patterns

### Query Examples

```csharp
// Get by ID
var user = await _context.Users.FindAsync(userId);

// Get by email
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

// Get with related data
var user = await _context.Users
    .Include(u => u.RefreshTokens)
    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

// Check existence
var exists = await _context.Users
    .AnyAsync(u => u.Email == email, cancellationToken);

// Get list with filtering
var users = await _context.Users
    .Where(u => u.IsActive)
    .OrderBy(u => u.Email)
    .ToListAsync(cancellationToken);

// Pagination
var users = await _context.Users
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
```

### Update Examples

```csharp
// Update tracked entity (no Update() call needed)
var user = await _context.Users.FindAsync(userId);
user.Email = "new@email.com";
await _context.SaveChangesAsync();

// Update untracked entity
var user = new User { Id = userId, Email = "new@email.com" };
_context.Users.Update(user);
await _context.SaveChangesAsync();

// Bulk update (EF Core 7+)
await _context.Users
    .Where(u => u.IsActive == false)
    .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsDeleted, true));
```

### Delete Examples

```csharp
// Delete tracked entity
var user = await _context.Users.FindAsync(userId);
_context.Users.Remove(user);
await _context.SaveChangesAsync();

// Delete untracked entity
_context.Users.Remove(new User { Id = userId });
await _context.SaveChangesAsync();

// Bulk delete (EF Core 7+)
await _context.Users
    .Where(u => u.IsDeleted)
    .ExecuteDeleteAsync();
```

### Transaction Examples

```csharp
// Automatic transaction (SaveChanges wraps in transaction)
user.Email = "new@email.com";
clinic.Name = "New Name";
await _context.SaveChangesAsync(); // Both saved in one transaction

// Manual transaction
using var transaction = await _context.BeginTransactionAsync();
try
{
    user.Email = "new@email.com";
    await _context.SaveChangesAsync();

    clinic.Name = "New Name";
    await _context.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

## Part 6: Migration Steps

1. **Create IApplicationDbContext interface** (shown above)
2. **Create ApplicationDbContext implementation** (shown above)
3. **Register in DI container**:
   ```csharp
   services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(connectionString));
   services.AddScoped<IApplicationDbContext>(sp =>
       sp.GetRequiredService<ApplicationDbContext>());
   ```
4. **Refactor handlers one by one** (start with Auth handlers)
5. **Update tests to use in-memory database**
6. **Remove old code**:
   - Delete all `IRepository` interfaces
   - Delete all repository implementations
   - Delete `IUnitOfWork` interface
   - Delete `UnitOfWork` implementation
   - Remove Dapper package

---

## Part 7: Testing Best Practices

### Use Unique Database Per Test

```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;
```

### Alternative: SQLite In-Memory (More Realistic)

```csharp
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlite(connection)
    .Options;

var context = new ApplicationDbContext(options);
context.Database.EnsureCreated();
```

### Test Base Class (Optional)

```csharp
public abstract class TestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}

// Usage
public class ChangePasswordHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Use Context from base class
        var user = new User { ... };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // ... rest of test
    }
}
```

---

## Summary

### What We're Removing:

- `IUnitOfWork` interface and implementation
- All `IRepository` interfaces (IUserRepository, etc.)
- All repository implementations
- Dapper SQL queries
- ~3000 lines of boilerplate code

### What We're Adding:

- `IApplicationDbContext` interface (simple)
- `ApplicationDbContext` implementation (EF Core)
- Direct LINQ queries in handlers
- In-memory database for tests

### Benefits:

✅ Simpler code (fewer abstractions)
✅ Type-safe queries (LINQ)
✅ Automatic migrations
✅ Change tracking
✅ Better testing (real database behavior)
✅ Less code to maintain
✅ Industry standard pattern

### Next Steps:

1. Review this guide
2. Create proof-of-concept with one handler (ChangePasswordHandler)
3. Verify tests work with in-memory database
4. Migrate remaining handlers
5. Remove old repository code
