# Clean Architecture Migration Guide

## What's Been Done

I've set up the initial Clean Architecture structure with three new projects and migrated the Specializations feature as a template for you to follow.

### Project Structure

```
ClinicManagement.sln
├── src/
│   ├── ClinicManagement.Domain/          # NEW - Pure business logic
│   ├── ClinicManagement.Application/     # NEW - Use cases (Commands/Queries)
│   ├── ClinicManagement.Infrastructure/  # NEW - Data access, external services
│   └── ClinicManagement.API/            # EXISTING - Endpoints, middleware
└── tests/
    ├── ClinicManagement.Tests/
    └── ClinicManagement.IntegrationTests/
```

### Dependencies Flow

```
Domain (no dependencies)
   ↑
Application (depends on Domain)
   ↑
Infrastructure (depends on Application + Domain)
   ↑
API (depends on Infrastructure)
```

## Specializations Feature (Your Template)

I've migrated the Specializations feature to show you the pattern:

### Query Example: GetSpecializations

**1. Query Object** (`Application/Features/Specializations/Queries/GetSpecializations/GetSpecializationsQuery.cs`)

```csharp
public record GetSpecializationsQuery : IRequest<List<SpecializationDto>>;
```

**2. DTO** (`SpecializationDto.cs`)

```csharp
public record SpecializationDto(
    Guid Id,
    string NameEn,
    string NameAr
);
```

**3. Handler** (`GetSpecializationsHandler.cs`)

```csharp
public class GetSpecializationsHandler : IRequestHandler<GetSpecializationsQuery, List<SpecializationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSpecializationsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SpecializationDto>> Handle(GetSpecializationsQuery request, CancellationToken cancellationToken)
    {
        var specializations = await _context.Set<Specialization>()
            .OrderBy(s => s.NameEn)
            .Select(s => new SpecializationDto(s.Id, s.NameEn, s.NameAr))
            .ToListAsync(cancellationToken);

        return specializations;
    }
}
```

**4. Endpoint** (Will be updated to use MediatR)

```csharp
app.MapGet("/specializations", async (ISender sender) =>
{
    var result = await sender.Send(new GetSpecializationsQuery());
    return Results.Ok(result);
});
```

## Next Steps for You

### Step 1: Complete Infrastructure Setup

You need to:

1. Move `ApplicationDbContext` from API to Infrastructure project
2. Make it implement `IApplicationDbContext`
3. Update all entity namespaces to use Domain entities
4. Create `DependencyInjection.cs` in Infrastructure to register services

### Step 2: Update API to Use MediatR

Update the Specializations endpoints to use MediatR:

```csharp
app.MapGet("/specializations", async (ISender sender) =>
{
    var result = await sender.Send(new GetSpecializationsQuery());
    return Results.Ok(result);
})
.RequireAuthorization();
```

### Step 3: Test the Specializations Feature

Run the app and test that the endpoints still work with the new architecture.

### Step 4: Migrate More Features

Follow the same pattern for other features. Suggested order:

1. **ChronicDiseases** (simple CRUD, similar to Specializations)
2. **Auth** (Login, Register) - more complex, good learning
3. **Patients** (CRUD with relationships)
4. **Appointments** (complex business logic)

## Pattern to Follow

### For Queries (Read Operations)

**Folder Structure:**

```
Features/
  └── [FeatureName]/
      └── Queries/
          └── [QueryName]/
              ├── [QueryName]Query.cs
              ├── [QueryName]Handler.cs
              ├── [QueryName]Validator.cs (if needed)
              └── [Response]Dto.cs
```

**Example: GetPatientById**

```csharp
// Query
public record GetPatientByIdQuery(Guid Id) : IRequest<PatientDto?>;

// Handler
public class GetPatientByIdHandler : IRequestHandler<GetPatientByIdQuery, PatientDto?>
{
    private readonly IApplicationDbContext _context;

    public async Task<PatientDto?> Handle(GetPatientByIdQuery request, CancellationToken ct)
    {
        return await _context.Set<Patient>()
            .Where(p => p.Id == request.Id)
            .Select(p => new PatientDto(...))
            .FirstOrDefaultAsync(ct);
    }
}

// Endpoint
app.MapGet("/patients/{id}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetPatientByIdQuery(id));
    return result is null ? Results.NotFound() : Results.Ok(result);
});
```

### For Commands (Write Operations)

**Folder Structure:**

```
Features/
  └── [FeatureName]/
      └── Commands/
          └── [CommandName]/
              ├── [CommandName]Command.cs
              ├── [CommandName]Handler.cs
              └── [CommandName]Validator.cs
```

**Example: CreatePatient**

```csharp
// Command
public record CreatePatientCommand(
    string FirstName,
    string LastName,
    DateTime DateOfBirth
) : IRequest<Guid>;

// Validator
public class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaxLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaxLength(100);
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.Today);
    }
}

// Handler
public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public async Task<Guid> Handle(CreatePatientCommand request, CancellationToken ct)
    {
        var patient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth
        };

        _context.Set<Patient>().Add(patient);
        await _context.SaveChangesAsync(ct);

        return patient.Id;
    }
}

// Endpoint
app.MapPost("/patients", async (CreatePatientCommand command, ISender sender) =>
{
    var id = await sender.Send(command);
    return Results.Created($"/patients/{id}", new { id });
})
.RequireAuthorization();
```

## Unit Testing (Your Learning Task)

I've left unit testing for you to learn. Here's how to test handlers:

### Setup Test Project

```bash
dotnet new xunit -n ClinicManagement.UnitTests
dotnet add reference ../src/ClinicManagement.Application
dotnet add package Moq
dotnet add package FluentAssertions
```

### Example Unit Test

```csharp
public class GetSpecializationsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllSpecializations_OrderedByNameEn()
    {
        // Arrange
        var mockContext = new Mock<IApplicationDbContext>();
        var specializations = new List<Specialization>
        {
            new() { Id = Guid.NewGuid(), NameEn = "Cardiology", NameAr = "القلب" },
            new() { Id = Guid.NewGuid(), NameEn = "Dermatology", NameAr = "الجلدية" }
        }.AsQueryable();

        var mockSet = CreateMockDbSet(specializations);
        mockContext.Setup(c => c.Set<Specialization>()).Returns(mockSet.Object);

        var handler = new GetSpecializationsHandler(mockContext.Object);
        var query = new GetSpecializationsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].NameEn.Should().Be("Cardiology");
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }
}
```

## Tips

1. **Start Small**: Migrate one feature at a time, test it, then move to the next
2. **Keep It Simple**: Don't over-engineer - handlers can be simple
3. **Use Records**: For DTOs and queries/commands (immutable, concise)
4. **Validation**: Add FluentValidation validators for commands
5. **Testing**: Write unit tests for handlers with complex logic
6. **Integration Tests**: Keep your existing integration tests - they're valuable!

## Common Mistakes to Avoid

❌ **Don't** put business logic in endpoints
✅ **Do** put it in handlers

❌ **Don't** return entities from handlers
✅ **Do** return DTOs

❌ **Don't** use DbContext directly in endpoints
✅ **Do** use ISender to send queries/commands

❌ **Don't** create generic repositories
✅ **Do** use IApplicationDbContext directly

## Questions?

Check `MIGRATION_PROGRESS.md` to see what's been done and what's next.

The Specializations feature is your complete template - copy its structure for other features!
