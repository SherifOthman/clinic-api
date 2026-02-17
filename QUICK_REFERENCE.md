# Clean Architecture Quick Reference

## File Naming Conventions

### Queries (Read)

```
GetSpecializations/
  ├── GetSpecializationsQuery.cs
  ├── GetSpecializationsHandler.cs
  └── SpecializationDto.cs

GetSpecializationById/
  ├── GetSpecializationByIdQuery.cs
  └── GetSpecializationByIdHandler.cs
```

### Commands (Write)

```
CreatePatient/
  ├── CreatePatientCommand.cs
  ├── CreatePatientHandler.cs
  └── CreatePatientValidator.cs

UpdatePatient/
  ├── UpdatePatientCommand.cs
  ├── UpdatePatientHandler.cs
  └── UpdatePatientValidator.cs
```

## Code Templates

### Query

```csharp
// Query
public record GetXQuery(Guid Id) : IRequest<XDto?>;

// Handler
public class GetXHandler : IRequestHandler<GetXQuery, XDto?>
{
    private readonly IApplicationDbContext _context;

    public GetXHandler(IApplicationDbContext context) => _context = context;

    public async Task<XDto?> Handle(GetXQuery request, CancellationToken ct)
    {
        return await _context.Set<X>()
            .Where(x => x.Id == request.Id)
            .Select(x => new XDto(...))
            .FirstOrDefaultAsync(ct);
    }
}
```

### Command

```csharp
// Command
public record CreateXCommand(string Name) : IRequest<Guid>;

// Validator
public class CreateXValidator : AbstractValidator<CreateXCommand>
{
    public CreateXValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaxLength(100);
    }
}

// Handler
public class CreateXHandler : IRequestHandler<CreateXCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateXHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateXCommand request, CancellationToken ct)
    {
        var entity = new X { Name = request.Name };
        _context.Set<X>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity.Id;
    }
}
```

### Endpoint

```csharp
// Query endpoint
app.MapGet("/api/x/{id}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetXQuery(id));
    return result is null ? Results.NotFound() : Results.Ok(result);
});

// Command endpoint
app.MapPost("/api/x", async (CreateXCommand command, ISender sender) =>
{
    var id = await sender.Send(command);
    return Results.Created($"/api/x/{id}", new { id });
});
```

## Common Validation Rules

```csharp
// Required string with max length
RuleFor(x => x.Name).NotEmpty().MaxLength(100);

// Optional string with max length
RuleFor(x => x.Description).MaxLength(500).When(x => x.Description != null);

// Email
RuleFor(x => x.Email).NotEmpty().EmailAddress();

// Phone (optional)
RuleFor(x => x.Phone).Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrEmpty(x.Phone));

// Date in past
RuleFor(x => x.DateOfBirth).LessThan(DateTime.Today);

// Date in future
RuleFor(x => x.AppointmentDate).GreaterThan(DateTime.Now);

// Guid not empty
RuleFor(x => x.PatientId).NotEmpty();

// Enum valid
RuleFor(x => x.Status).IsInEnum();

// Decimal positive
RuleFor(x => x.Amount).GreaterThan(0);

// Custom rule
RuleFor(x => x.Password)
    .NotEmpty()
    .MinimumLength(8)
    .Matches(@"[A-Z]").WithMessage("Must contain uppercase")
    .Matches(@"[a-z]").WithMessage("Must contain lowercase")
    .Matches(@"\d").WithMessage("Must contain digit");
```

## Git Workflow

```bash
# Check current branch
git branch

# Switch to main to see original code
git checkout main

# Switch back to clean architecture
git checkout feature/clean-architecture

# Commit your work
git add .
git commit -m "feat: Migrate [FeatureName] to Clean Architecture"
git push
```

## Build & Test

```bash
# Build solution
dotnet build

# Run API
dotnet run --project src/ClinicManagement.API

# Run integration tests
dotnet test tests/ClinicManagement.IntegrationTests

# Run unit tests (when you create them)
dotnet test tests/ClinicManagement.UnitTests
```

## Checklist for Migrating a Feature

- [ ] Create Domain entity (if not exists)
- [ ] Create Query/Command object
- [ ] Create Handler
- [ ] Create Validator (for commands)
- [ ] Create DTO (for responses)
- [ ] Update endpoint to use ISender
- [ ] Test manually
- [ ] Write unit tests (your learning task)
- [ ] Update integration tests if needed
- [ ] Commit and push

## Where Things Go

| What       | Where                                      |
| ---------- | ------------------------------------------ |
| Entities   | `Domain/Entities/`                         |
| Enums      | `Domain/Enums/`                            |
| Exceptions | `Domain/Exceptions/`                       |
| Queries    | `Application/Features/[Feature]/Queries/`  |
| Commands   | `Application/Features/[Feature]/Commands/` |
| DTOs       | Same folder as query/command               |
| Validators | Same folder as command                     |
| Endpoints  | `API/Endpoints/` or `API/Features/`        |
| DbContext  | `Infrastructure/Data/`                     |
| Services   | `Infrastructure/Services/`                 |

## Need Help?

1. Look at `Specializations` feature - it's your complete template
2. Check `CLEAN_ARCHITECTURE_GUIDE.md` for detailed explanations
3. Check `MIGRATION_PROGRESS.md` to see what's done
4. Compare with `main` branch to see original code
