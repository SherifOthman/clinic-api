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
- 5 repository interfaces
- Result pattern for error handling
- Zero external dependencies

### Application Layer

- **Features**: Auth (12 commands + 3 queries), SubscriptionPlans (1 query)
- **Abstractions**: 9 interfaces (IPasswordHasher, ITokenService, IEmailService, IFileStorageService, etc.)
- **Pipeline Behaviors**: Logging, Validation, Performance
- CQRS with MediatR
- FluentValidation for input validation

### Infrastructure Layer

- **Data Access**: Dapper + 3 repositories (User, RefreshToken, SubscriptionPlan) + UnitOfWork + DbUp migrations
- **17 Services**: Authentication (PasswordHasher, TokenService, TokenGenerator, CookieService), Email (EmailService, EmailConfirmationService, SmtpEmailSender, MailKitSmtpClient, EmailTemplates), Storage (LocalFileStorageService), Identity (RefreshTokenService, SuperAdminSeedService), Utilities (CurrentUserService, DateTimeProvider, PhoneValidationService, GeoNamesService), Background (RefreshTokenCleanupService)

### API Layer

- **4 Controllers**: AuthController (15 endpoints), LocationsController (3 endpoints), SubscriptionPlansController (1 endpoint), BaseApiController
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
- Standardized error codes for frontend i18n
- Structured logging with Serilog

---

## What Makes This Project Stand Out

### Clean Architecture Implementation

- Proper layer separation with dependency inversion (all dependencies point inward)
- Domain layer has zero external dependencies (pure C#)
- Application layer defines abstractions, Infrastructure implements them
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
- Repository pattern with Dapper (lightweight, performant)
- FluentValidation for complex business rules
- DbUp for version-controlled SQL migrations

---

## Tech Stack

- .NET 10
- Dapper (data access)
- DbUp (migrations)
- MediatR (CQRS)
- FluentValidation
- BCrypt.Net-Next (password hashing)
- JWT Bearer Authentication
- Serilog (logging)
- Scalar (API docs)
- MailKit (SMTP)
- GeoNames API (location data)

---

## Skills Demonstrated

- Clean Architecture & SOLID principles
- CQRS pattern with MediatR
- Multi-tenant SaaS design
- Repository pattern with Dapper
- JWT authentication with refresh tokens
- Email workflows (confirmation, password reset)
- Result pattern for error handling
- FluentValidation
- Structured logging with Serilog
- Background services
- RESTful API design
- SQL migrations with DbUp

---

## License

MIT
