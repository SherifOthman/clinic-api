# Clinic Management API

Multi-tenant SaaS backend for clinic operations built with .NET 10 and Clean Architecture.

**Live Demo**: http://clinic-api.runasp.net/scalar/v1  
**Dashboard**: https://clinic-dashboard-ecru.vercel.app  
**Website**: https://clinic-website-lime.vercel.app

**Repositories**: [API](https://github.com/SherifOthman/clinic-api) • [Dashboard](https://github.com/SherifOthman/clinic-dashboard) • [Website](https://github.com/SherifOthman/clinic-website)

---

## Architecture

Clean Architecture with 4 layers and proper dependency flow (dependencies point inward):

```
API Layer (Controllers, Middleware)
    ↓ depends on
Infrastructure Layer (Repositories, Services, External APIs)
    ↓ depends on
Application Layer (Use Cases, Commands, Queries, Behaviors)
    ↓ depends on
Domain Layer (Entities, Domain Logic)
    NO DEPENDENCIES
```

### Domain Layer

- 43 entities across 9 modules (Appointment, Billing, Clinic, Identity, Inventory, Medical, Patient, Reference, Staff)
- 14 enums (Gender, AppointmentStatus, InvoiceStatus, PaymentMethod, etc.)
- Result pattern for error handling
- Zero external dependencies

### Application Layer

- **Features**: Auth (12 commands + 3 queries), Patients (3 commands + 1 query), Reference (1 query for chronic diseases), Onboarding (1 command), Staff (2 commands + 2 queries)
- **Abstractions**: 9 interfaces (IPasswordHasher, ITokenService, IEmailService, IFileStorageService, etc.)
- **Pipeline Behaviors**: Logging, Validation, Performance
- **Mapping**: Mapster for object-to-object mapping with custom configurations
- CQRS with MediatR
- FluentValidation for input validation

### Infrastructure Layer

- **Data Access**: EF Core with ApplicationDbContext, ASP.NET Identity, Entity Configurations, and direct DbContext access (no repository pattern)
- **17 Services**: Authentication (TokenService, CookieService), Email (EmailService, EmailTokenService, SmtpEmailSender, MailKitSmtpClient, EmailTemplates), Storage (LocalFileStorageService), Identity (RefreshTokenService, SuperAdminSeedService, ClinicOwnerSeedService), Utilities (CurrentUserService, DateTimeProvider, PhoneValidationService, GeoNamesService), Background (RefreshTokenCleanupService, EmailQueueProcessorJob, SubscriptionExpiryNotificationJob, UsageMetricsAggregationJob)

### API Layer

- **9 Controllers**: AuthController (15 endpoints), PatientsController (4 endpoints), ChronicDiseasesController (1 endpoint), LocationsController (3 endpoints), SpecializationsController (1 endpoint), SubscriptionPlansController (1 endpoint), OnboardingController (1 endpoint), StaffController (3 endpoints), BaseApiController
- Global exception middleware
- JWT authentication
- Swagger/Scalar documentation
- Serilog logging

---

## Key Features

### Authentication & Authorization

- Hybrid token architecture (web: HTTP-only cookies, mobile: response body)
- Access tokens (60 min) + refresh tokens (30 days) with rotation
- Email confirmation workflow
- Password reset via email token
- Profile management with image upload
- Role-based authorization (ClinicOwner, Doctor, Receptionist, SuperAdmin)

### Multi-Tenancy

- Every tenant-scoped entity has `ClinicId` property
- User JWT contains `ClinicId` claim
- `CurrentUserService` extracts `ClinicId` from JWT
- **Note**: Automatic query filtering not yet implemented (planned for future)

### Error Handling

- Result pattern (no exceptions for flow control)
- Global exception middleware with RFC 7807 Problem Details
- Standardized error codes (constants) for frontend i18n
- Structured logging with Serilog

---

## What Makes This Project Stand Out

### Clean Architecture Implementation

- Proper layer separation with dependency inversion (all dependencies point inward)
- Domain layer has zero external dependencies (pure C#)
- Application layer defines abstractions (IApplicationDbContext), Infrastructure implements them
- Direct EF Core DbContext usage in handlers (no repository pattern overhead)
- Highly testable: all dependencies are interfaces with constructor injection

### CQRS Pattern with MediatR

- Commands for writes (validation, business logic, side effects)
- Queries for reads (optimized projections)
- Pipeline behaviors for cross-cutting concerns (logging, validation, performance tracking)

### Multi-Tenant SaaS Architecture

- Designed for data isolation from day one
- Every tenant-scoped entity has `ClinicId` property
- JWT claims include tenant context
- Ready for automatic query filtering when needed

### Production-Ready Features

- Hybrid token strategy (web: HTTP-only cookies, mobile: response body)
- Token rotation for security
- Email confirmation workflow
- Global exception handling with RFC 7807 Problem Details
- Structured logging with Serilog
- Background services for cleanup tasks

### Modern .NET Practices

- Result pattern instead of exceptions for flow control
- Direct EF Core DbContext access (no repository pattern abstraction)
- EF Core with Include, Where, ProjectToType for optimized queries
- ASP.NET Identity for authentication and authorization
- FluentValidation for complex business rules
- Entity configurations for database schema
- Mapster for efficient object mapping

---

## Tech Stack

- .NET 10
- EF Core (data access)
- ASP.NET Identity (authentication)
- MediatR (CQRS)
- Mapster (object mapping)
- FluentValidation
- JWT Bearer Authentication
- Serilog (logging)
- Scalar (API docs)
- MailKit (SMTP)
- libphonenumber-csharp (phone validation)
- GeoNames API (location data)

---

## Skills Demonstrated

- Clean Architecture & SOLID principles
- CQRS pattern with MediatR
- Multi-tenant SaaS design
- EF Core with direct DbContext access (no repository pattern)
- ASP.NET Identity integration
- JWT authentication with refresh tokens
- Email workflows (confirmation, password reset)
- Result pattern for error handling
- Object mapping with Mapster
- FluentValidation
- Structured logging with Serilog
- Background services
- RESTful API design
- Entity configurations
- Phone number validation with libphonenumber

---

## License

MIT
