# 🏥 Clinic Management System API

A production-ready healthcare management system built with **.NET 10** and **Clean Architecture**. This API powers multi-tenant clinic operations including patient management, appointments, billing, and inventory tracking.

**🚀 Live Demo**: https://clinic-api.runasp.net/swagger

## 💡 What Makes This Project Stand Out

This isn't just another CRUD API. It demonstrates:

- **Real-world complexity**: Multi-tenant SaaS with subscription-based access control
- **Clean Architecture**: Proper separation of concerns across 4 layers
- **Domain-Driven Design**: Rich domain models with business logic in entities
- **Modern .NET practices**: CQRS, MediatR, Repository Pattern, Unit of Work, FluentValidation
- **Production features**: JWT auth, file uploads, email verification, background services
- **Pragmatic decisions**: Repository pattern with switch expressions, no reflection-based sorting

## 🛠️ Tech Stack

- **.NET 10** with C# 13
- **Entity Framework Core** - Code-first with migrations
- **SQL Server** - 40 entities, 3 migrations
- **MediatR** - CQRS pattern (38 handlers)
- **FluentValidation** - Input validation
- **JWT + Refresh Tokens** - Secure authentication
- **ASP.NET Core Identity** - User management
- **Serilog** - Structured logging
- **Mapster** - Object mapping

## 🏗️ Architecture

### Clean Architecture (4 Layers)

```
API Layer          → Controllers with MediatR, middleware
Application Layer  → CQRS handlers, DTOs, validation
Domain Layer       → 40 entities with business logic, repository interfaces
Infrastructure     → EF Core, repositories, Unit of Work, external services
```

### Key Design Patterns

**Repository Pattern with Unit of Work**

```csharp
public interface IUnitOfWork : IDisposable
{
    // Specific repositories with custom methods
    IPatientRepository Patients { get; }
    IMedicineRepository Medicines { get; }
    IAppointmentRepository Appointments { get; }

    // Generic repository for simple entities
    IRepository<T> Repository<T>() where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// Usage in handlers
public class CreatePatientCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<PatientDto>> Handle(...)
    {
        var patient = new Patient(...);
        await _unitOfWork.Patients.AddAsync(patient);
        await _unitOfWork.SaveChangesAsync();
        return Result<PatientDto>.Ok(patientDto);
    }
}
```

**Switch Expression Sorting (No Reflection)**

```csharp
protected override IQueryable<Patient> ApplySorting(
    IQueryable<Patient> query,
    SearchablePaginationRequest request)
{
    return request.SortBy?.ToLower() switch
    {
        "fullname" => request.IsAscending
            ? query.OrderBy(p => p.FullName)
            : query.OrderByDescending(p => p.FullName),
        "patientcode" => request.IsAscending
            ? query.OrderBy(p => p.PatientCode)
            : query.OrderByDescending(p => p.PatientCode),
        _ => query.OrderByDescending(p => p.CreatedAt)
    };
}
```

**Rich Domain Model**

```csharp
public class Appointment : AuditableEntity
{
    // Calculated properties (business logic)
    public decimal RemainingAmount => FinalPrice - DiscountAmount - PaidAmount;
    public bool IsFullyPaid => RemainingAmount <= 0;

    // Business methods (state transitions)
    public void ApplyDiscount(decimal amount)
    {
        if (amount > FinalPrice)
            throw new InvalidDiscountException(...);
        DiscountAmount = amount;
    }
}
```

**CQRS with MediatR**

```csharp
// Command + Handler in one file
public record CreateAppointmentCommand(...) : IRequest<Result<AppointmentDto>>;

public class CreateAppointmentCommandHandler : IRequestHandler<...>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<AppointmentDto>> Handle(...)
    {
        // Business logic using Unit of Work
        var appointment = new Appointment(...);
        await _unitOfWork.Appointments.AddAsync(appointment);
        await _unitOfWork.SaveChangesAsync();
        return Result<AppointmentDto>.Ok(dto);
    }
}

// Validator in separate file
public class CreateAppointmentCommandValidator : AbstractValidator<...> { }
```

**Controllers with MediatR**

```csharp
[ApiController]
[Route("api/appointments")]
public class AppointmentsController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateAppointmentCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
```

## ✨ Features

### Core Functionality

- **Multi-tenant architecture** - Multiple clinics with isolated data
- **Subscription management** - 4 plans (Starter, Basic, Professional, Enterprise)
- **Patient management** - Demographics, medical history, chronic diseases
- **Appointment system** - Scheduling, queue management, status tracking
- **Billing & invoicing** - Invoice generation, payment processing, discounts
- **Inventory management** - Medicines, supplies, stock tracking
- **Medical records** - Vital signs, measurements, prescriptions

### Technical Features

- **JWT authentication** with refresh token rotation
- **Email verification** and password reset
- **File uploads** - Profile images with validation
- **Role-based authorization** - SuperAdmin, ClinicOwner, Doctor, Receptionist
- **Background services** - Refresh token cleanup
- **Global exception handling** - Domain and global middleware
- **Pagination** - Efficient data retrieval
- **GeoNames integration** - Location services

## 📊 Project Stats

- **40 entities** across 6 domains (Identity, Clinic, Patient, Appointment, Inventory, Billing)
- **17 controllers** with 40+ endpoints
- **38 CQRS handlers** (Commands + Queries)
- **9 specific repositories** with custom methods
- **Generic Repository<T>** for simple entities
- **4 subscription plans** with feature-based access control
- **10 specializations**, 10 chronic diseases, 10 vital signs, 8 appointment types

## 🚀 Quick Start

### Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or full instance)

### Setup

```bash
# Clone the repository
git clone https://github.com/SherifOthman/clinic-api.git
cd clinic-api

# Update connection string in appsettings.Development.json

# Run migrations
dotnet ef database update --project src/ClinicManagement.Infrastructure --startup-project src/ClinicManagement.API

# Run the application
dotnet run --project src/ClinicManagement.API

# Access Swagger
# https://localhost:7001/swagger
```

### Default Credentials

**Super Admin**

- Email: `superadmin@clinic.com`
- Password: `SuperAdmin123!`

## 📁 Project Structure

```
src/
├── ClinicManagement.API/              # Presentation Layer
│   ├── Controllers/                   # 17 controllers
│   ├── Middleware/                    # Exception handling, JWT
│   └── Program.cs
│
├── ClinicManagement.Application/      # Application Layer
│   ├── Features/                      # CQRS handlers by feature
│   │   ├── Auth/
│   │   ├── Appointments/
│   │   ├── Patients/
│   │   └── ...
│   ├── DTOs/
│   └── Common/
│       ├── Interfaces/
│       ├── Behaviors/                 # MediatR pipeline
│       └── Models/                    # Result, PagedResult
│
├── ClinicManagement.Domain/           # Domain Layer
│   ├── Entities/                      # 40 entities with business logic
│   │   ├── Appointment/
│   │   ├── Patient/
│   │   ├── Billing/
│   │   └── ...
│   └── Common/
│       ├── Interfaces/                # IRepository, IUnitOfWork
│       ├── Exceptions/                # Domain exceptions
│       └── Enums/
│
└── ClinicManagement.Infrastructure/   # Infrastructure Layer
    ├── Data/
    │   ├── ApplicationDbContext.cs
    │   ├── Repositories/              # 9 specific + BaseRepository
    │   ├── UnitOfWork.cs
    │   ├── Configurations/            # EF entity configs
    │   └── Migrations/
    └── Services/                      # External services
```

## 🎯 Key Learnings & Decisions

### What I Learned

- Implementing Clean Architecture in a real-world scenario
- CQRS pattern with MediatR for scalable command/query separation
- Repository and Unit of Work pattern for data access abstraction
- Rich domain models vs anemic models
- Multi-tenancy with data isolation
- Subscription-based SaaS architecture
- Domain-driven design principles

### Pragmatic Decisions

- **Repository Pattern**: Specific repositories for complex queries, generic for simple CRUD
- **Switch Expression Sorting**: Type-safe, performant sorting without reflection
- **Dictionary-Based Filtering**: Flexible filtering for large transactional tables
- **Command/Query + Handler in one file**: Better cohesion, easier navigation
- **Validators in separate files**: Reusability and single responsibility
- **Controllers with MediatR**: Clean separation, no business logic in controllers

## 🔒 Security Features

- JWT token authentication with refresh tokens
- Password hashing with ASP.NET Core Identity
- Email verification workflow
- Role-based authorization
- Clinic data isolation (multi-tenancy)
- Input validation with FluentValidation
- SQL injection protection (EF Core parameterization)
- CORS configuration
- HTTPS enforcement

## 📄 API Documentation

**Live Swagger**: https://clinic-api.runasp.net/swagger

### Controllers

- **AuthController** - Register, login, password reset, profile management
- **OnboardingController** - Clinic setup, subscription plans
- **StaffInvitationsController** - Staff invitation management
- **AppointmentsController** - Appointment CRUD and management
- **PatientsController** - Patient management with pagination
- **PatientChronicDiseasesController** - Patient chronic disease management
- **ChronicDiseasesController** - Chronic disease catalog
- **SpecializationsController** - Medical specializations
- **MeasurementsController** - Vital signs and measurements
- **MedicinesController** - Medicine inventory management
- **MedicalSuppliesController** - Medical supplies inventory
- **MedicalServicesController** - Medical services catalog
- **InvoicesController** - Invoice generation and management
- **PaymentsController** - Payment processing
- **LocationsController** - GeoNames integration for locations
- **SubscriptionPlansController** - Subscription plan management

## 🧪 Testing Approach

The application is designed for testability:

- **Unit tests**: Domain entities and business logic
- **Integration tests**: CQRS handlers with in-memory database
- **API tests**: Endpoint testing with WebApplicationFactory

## 📄 License

MIT License

---

**Built by**: Sherif Othman  
**Repository**: https://github.com/SherifOthman/clinic-api  
**Live Demo**: https://clinic-api.runasp.net/swagger

_This project demonstrates production-ready .NET development with Clean Architecture, CQRS, and Domain-Driven Design principles._
