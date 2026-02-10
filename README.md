# 🏥 Clinic Management System API

A production-ready healthcare management system built with **.NET 10** and **Clean Architecture**. This API powers multi-tenant clinic operations including patient management, appointments, billing, and inventory tracking.

**🚀 Live Demo**: https://clinic-api.runasp.net/swagger

## 💡 What Makes This Project Stand Out

This isn't just another CRUD API. It demonstrates:

- **Real-world complexity**: Multi-tenant SaaS with subscription-based access control
- **Clean Architecture**: Proper separation of concerns across 4 layers
- **Domain-Driven Design**: Rich domain models with business logic in entities
- **Modern .NET practices**: CQRS, MediatR, FluentValidation, EF Core
- **Production features**: JWT auth, file uploads, email verification, background services
- **Pragmatic decisions**: No unnecessary abstractions (avoided Repository/UoW with EF Core)

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
API Layer          → Minimal API endpoints (15 groups), middleware
Application Layer  → CQRS handlers, DTOs, validation
Domain Layer       → 40 entities with business logic
Infrastructure     → EF Core, external services, Identity
```

### Key Design Patterns

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
    // Handler implementation
}

// Validator in separate file
public class CreateAppointmentCommandValidator : AbstractValidator<...> { }
```

**No Repository Pattern**

- EF Core's `DbContext` IS the Unit of Work
- `DbSet<T>` IS the Repository
- Direct usage avoids unnecessary abstraction

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
- **15 endpoint groups** with 40+ endpoints
- **38 CQRS handlers** (Commands + Queries)
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
│   ├── Endpoints/                     # 15 endpoint groups
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
│       ├── Exceptions/                # Domain exceptions
│       └── Enums/
│
└── ClinicManagement.Infrastructure/   # Infrastructure Layer
    ├── Data/
    │   ├── ApplicationDbContext.cs
    │   ├── Configurations/            # EF entity configs
    │   └── Migrations/
    └── Services/                      # External services
```

## 🎯 Key Learnings & Decisions

### What I Learned

- Implementing Clean Architecture in a real-world scenario
- CQRS pattern with MediatR for scalable command/query separation
- Rich domain models vs anemic models
- Multi-tenancy with data isolation
- Subscription-based SaaS architecture
- Domain-driven design principles

### Pragmatic Decisions

- **No Repository Pattern**: EF Core already provides this - avoiding over-engineering
- **Command/Query + Handler in one file**: Better cohesion, easier navigation
- **Validators in separate files**: Reusability and single responsibility
- **Direct EF Core usage**: Simpler, more maintainable than abstraction layers

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

## � API Documentation

**Live Swagger**: https://clinic-api.runasp.net/swagger

### Endpoint Groups

- Authentication (register, login, password reset, profile management)
- Onboarding (clinic setup, subscription plans)
- Staff Invitations
- Appointments & Appointment Types
- Patients & Patient Chronic Diseases
- Chronic Diseases
- Specializations
- Measurements (vital signs)
- Medicines, Medical Supplies, Medical Services
- Invoices & Payments
- Locations (GeoNames integration)

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
