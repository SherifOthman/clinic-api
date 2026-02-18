# Clinic Management API

## 🚀 Overview

Multi-tenant SaaS backend for clinic operations built with .NET 10 and Clean Architecture.

The system handles patient management, appointment scheduling, billing, inventory tracking, and medical records. Each clinic operates as an isolated tenant with automatic data scoping at the database level. Authentication uses JWT tokens with refresh token rotation, and authorization is role-based (ClinicOwner, Doctor, Receptionist).

The architecture follows Clean Architecture principles with proper layer separation (Domain, Application, Infrastructure, API) and CQRS pattern using MediatR. Business logic is organized by feature with clear separation of concerns and testability as a first-class citizen.

Core modules include authentication, patient records, appointments, invoicing with payment tracking, medicine inventory with expiration monitoring, and medical visit documentation with prescriptions and lab orders.

**Live Demo**: http://clinic-api.runasp.net/scalar/v1  
**Dashboard**: https://clinic-dashboard-ecru.vercel.app  
**Website**: https://clinic-website-lime.vercel.app

**Repositories**: [API](https://github.com/SherifOthman/clinic-api) • [Dashboard](https://github.com/SherifOthman/clinic-dashboard) • [Website](https://github.com/SherifOthman/clinic-website)

---

## 🏗 Architecture

**Clean Architecture with CQRS pattern.** The application is organized into four distinct layers with proper dependency flow and separation of concerns.

### Layer Structure

```
┌─────────────────────────────────────────────┐
│              API Layer                      │
│  (Controllers, Middleware, Configuration)   │
└─────────────────┬───────────────────────────┘
                  │ depends on
                  ▼
┌─────────────────────────────────────────────┐
│         Infrastructure Layer                │
│  (Repositories, Services, External APIs)    │
└─────────────────┬───────────────────────────┘
                  │ depends on
                  ▼
┌─────────────────────────────────────────────┐
│         Application Layer                   │
│  (Use Cases, Commands, Queries, Behaviors)  │
└─────────────────┬───────────────────────────┘
                  │ depends on
                  ▼
┌─────────────────────────────────────────────┐
│            Domain Layer                     │
│  (Entities, Value Objects, Domain Logic)    │
│           NO DEPENDENCIES                   │
└─────────────────────────────────────────────┘
```

### Key Architectural Decisions

**Domain Layer (Core):**

- 50+ entities organized by domain concern (Appointment, Billing, Clinic, Identity, Inventory, Medical, Patient, Reference, Staff)
- Repository interfaces (IUserRepository, IRefreshTokenRepository, ISubscriptionPlanRepository)
- Result pattern for error handling
- Zero external dependencies (pure C#)

**Application Layer (Use Cases):**

- CQRS pattern with MediatR (Commands for writes, Queries for reads)
- Feature-based organization (Auth, SubscriptionPlans at root level - Screaming Architecture)
- Abstractions folder for output ports (interfaces to Infrastructure)
- Pipeline behaviors (Logging, Validation, Performance monitoring)
- FluentValidation for input validation
- All handlers return Result<T> (no exceptions for flow control)

**Infrastructure Layer (External Concerns):**

- Dapper for data access (lightweight ORM)
- DbUp for SQL-based migrations
- Repository pattern implementations
- BCrypt for password hashing
- MailKit for email sending
- JWT token generation and validation

**API Layer (Presentation):**

- Controllers only (no business logic)
- Uses MediatR to send commands/queries
- Global exception middleware
- JWT authentication
- Swagger/Scalar documentation

### Design Patterns

- **CQRS**: Separate models for reads and writes
- **Repository Pattern**: Abstracts data access
- **Unit of Work**: Manages transactions
- **Result Pattern**: Explicit error handling without exceptions
- **Mediator Pattern**: Decouples controllers from handlers
- **Strategy Pattern**: Pluggable implementations (IPasswordHasher, IEmailService, etc.)

### Why Clean Architecture?

**Benefits achieved:**

- ✅ Testability: All dependencies are interfaces, easy to mock
- ✅ Maintainability: Clear separation of concerns
- ✅ Flexibility: Easy to swap implementations (e.g., Dapper → EF Core)
- ✅ Scalability: Feature-based organization makes it easy to add new features
- ✅ Independence: Business logic doesn't depend on frameworks or databases

**Trade-offs:**

- More files and folders (proper organization)
- More abstractions (interfaces for everything)
- Steeper learning curve for new developers

**Result:** 100% Clean Architecture compliance verified against industry best practices (Milan Jovanovic, Jason Taylor, Clean DDD).

---

## 🏢 Multi-Tenancy Strategy

**Data isolation** happens at the database level using EF Core global query filters. Every tenant-scoped entity (Patient, Invoice, Appointment, etc.) has a `ClinicId` foreign key.

**How it works:**

1. User logs in → JWT contains their `ClinicId` claim
2. `CurrentUserService` extracts `ClinicId` from the JWT on each request
3. EF Core automatically appends `WHERE ClinicId = @clinicId` to all queries
4. Developers cannot accidentally query cross-tenant data

**Implementation:**

- Entities inherit from `TenantEntity` base class which implements `ITenantEntity` interface
- Interface provides compile-time safety and explicit contract
- `SaveChangesAsync` override automatically sets `ClinicId` on new entities
- SuperAdmin role can bypass filters using `.IgnoreQueryFilters()` for system-wide operations

**Security guarantee:** No way to access another clinic's data without explicit filter bypass. All queries are tenant-scoped by default at the database level.

---

## 🔐 Authentication & Authorization

**Hybrid token architecture** supports both web (SPA) and mobile clients with different storage strategies:

**Web clients (SPA):**

- Access token (60-minute expiry) stored in memory (React Context)
- Refresh token (30-day expiry) stored in HTTP-only cookie
- Automatic token refresh via Axios interceptor on 401 responses

**Mobile clients:**

- Both tokens returned in response body
- Client stores tokens in secure storage
- Identified by `X-Client-Type: mobile` header

**Login flow:**

1. Validate credentials (email or username) using ASP.NET Identity
2. Generate access + refresh tokens with UserId, Email, Roles, and ClinicId claims
3. Web: Return access token in body, refresh token in HTTP-only cookie
4. Mobile: Return both tokens in response body
5. Frontend calls `/auth/me` to fetch user data

**Token refresh:** Per-user semaphore prevents race conditions during concurrent refresh requests. Old refresh token is revoked on use (rotation pattern). Background service cleans expired tokens daily.

**Role-based authorization:**

- ASP.NET Identity roles
- ClinicOwner: Full clinic management
- Doctor: Patient records, prescriptions, appointments
- Receptionist: Patient registration, scheduling
- SuperAdmin: System-wide operations (seeded only, never via registration)

**Staff architecture:** Staff table manages clinic membership for all roles. DoctorProfile table stores doctor-specific data (specialization, license, consultation fee). User table is pure identity with no clinic-specific data.

**Cookie security:** `HttpOnly` prevents XSS, `Secure` enforces HTTPS in production, `SameSite=None` for cross-origin requests.

---

## 🗄 Database Design

**50+ entities** organized across 9 domain modules: Identity (2), Clinic (4), Staff (2), Patient (4), Appointment (2), Billing (3), Inventory (6), Medical Records (15), Reference Data (3).

**Data Access:**

- Dapper for lightweight, performant data access
- Repository pattern for abstraction
- Unit of Work for transaction management
- DbUp for SQL-based migrations (version control for database)

**Key relationships:**

- One-to-Many: Clinic → Staff, Staff → DoctorProfile, Clinic → Patients, Patient → Appointments, Invoice → InvoiceItems, Invoice → Payments
- Many-to-Many: Patient ↔ ChronicDiseases, DoctorProfile ↔ MeasurementAttributes

**Staff architecture:** Staff table links Users to Clinics with role-based membership. DoctorProfile extends Staff with doctor-specific data (only created for users with Doctor role). This separates identity (User) from clinic membership (Staff) from role-specific data (DoctorProfile).

**Soft delete:** All entities inherit from `AuditableEntity` with `IsDeleted`, `DeletedAt`, `DeletedBy` fields. Queries automatically exclude soft-deleted records. This maintains referential integrity and enables data recovery.

**Audit tracking:** `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` automatically populated via repository implementations.

**Query optimization:**

- Foreign keys indexed automatically
- Composite indexes on frequently queried columns (e.g., `ClinicId + PatientCode`)
- Pagination on all list endpoints (default 10, max 100)
- Efficient SQL queries with Dapper (no N+1 problems)

---

## ⚙ Business Logic Highlights

**Appointment state machine:** Pending → Confirmed → Completed/Cancelled. Validation prevents invalid transitions (e.g., can't complete a non-confirmed appointment, cancelled appointments are terminal).

**Billing calculations:** Invoice tracks `SubtotalAmount` (sum of line items), `FinalAmount` (after discount and tax), `TotalPaid` (sum of payments), and `RemainingAmount`. Calculated properties include `IsFullyPaid`, `IsOverdue`, `IsPartiallyPaid`. Multiple partial payments supported with status tracking (Pending, Paid, Failed, Refunded).

**Inventory management:** Medicine stock uses box/strip system (e.g., 1 box = 10 strips). Tracks `TotalStripsInStock`, `FullBoxesInStock`, `RemainingStrips`. Expiry date monitoring and low stock alerts via calculated `IsLowStock` property. Stock operations validate against negative quantities.

**Domain rules enforced:**

- Cannot confirm already confirmed appointments
- Cannot dispense more stock than available
- Invoice due date must be after issue date
- Patient age calculated from date of birth

---

## 🛡 Error Handling & Logging

**Result Pattern** replaces exceptions for flow control. All handlers return `Result` or `Result<T>` with explicit success/failure states.

```csharp
// Handler
return Result.Success(data);
return Result.Failure<T>("ERROR_CODE", "Error message");

// Controller
if (result.IsFailure)
    return Error(result.ErrorCode!, result.ErrorMessage!, "Title");
return Ok(result.Value);
```

**Global exception middleware** catches unhandled exceptions and returns RFC 7807 Problem Details responses. Exception types map to appropriate HTTP status codes (401, 403, 404, 400, 500).

**Standardized error format:**

```json
{
  "code": "INSUFFICIENT_STOCK",
  "title": "Insufficient Stock",
  "status": 400,
  "detail": "Available: 5, Requested: 10",
  "traceId": "0HMVFE..."
}
```

**40+ predefined error codes** organized by category (Validation, Authentication, Authorization, Business Logic, System). Frontend uses these codes for i18n translation.

**Structured logging with Serilog:**

- Development: Debug level, console + file
- Production: Warning level, file only (3-day retention)
- Logs authentication attempts, entity changes, business rule violations, external service calls
- Trace IDs correlate frontend errors with backend logs
- Pipeline behavior logs all requests with performance metrics

---

## 📦 API Design

**REST semantics:** GET (retrieve), POST (create), PUT (update), DELETE (soft delete). Status codes follow HTTP standards (200, 201, 204, 400, 401, 403, 404, 500).

**Endpoint naming:** Plural nouns (`/patients`, `/appointments`), resource IDs in path (`/patients/{id}`), actions as verbs (`/appointments/{id}/confirm`), query parameters for filtering.

**DTO separation:** Request DTOs have validation attributes and flat structure. Response DTOs include calculated fields and related data via projections. This decouples API contract from database schema and prevents over-posting.

**Validation:**

- FluentValidation for complex business rules
- Data annotations for simple validations
- Pipeline behavior validates all requests before handler execution
- Returns 400 with detailed validation errors

**CQRS Benefits:**

- Commands: Optimized for writes (validation, business logic, side effects)
- Queries: Optimized for reads (projections, filtering, pagination)
- Clear separation of concerns
- Easy to optimize independently

---

## 📊 Project Scope

**19 API endpoints** across authentication and reference data: Authentication (15 endpoints), Locations (3 endpoints), Subscription Plans (1 endpoint).

**Technology stack:**

- .NET 10
- Dapper (data access)
- DbUp (migrations)
- MediatR (CQRS)
- FluentValidation
- BCrypt (password hashing)
- JWT Bearer Authentication
- Serilog (logging)
- Scalar (API docs)
- MailKit (SMTP)
- GeoNames API

**Infrastructure:**

- 4 layers (Domain, Application, Infrastructure, API)
- 50+ domain entities
- 3 repositories with Unit of Work
- 17 infrastructure services
- 2 custom middleware
- 3 pipeline behaviors
- Background service for token cleanup

**Testing:**

- Highly testable architecture (all dependencies are interfaces)
- Constructor injection throughout
- Result pattern for easy assertions
- Ready for unit, integration, and API tests
- See `TESTABILITY_GUIDE.md` for examples

---

## 🧠 What I Learned

**Clean Architecture implementation:** Understanding the proper separation of concerns across Domain, Application, Infrastructure, and API layers. Learning when and why to use abstractions, and how to maintain proper dependency flow (dependencies always point inward).

**CQRS pattern:** Implementing Command Query Responsibility Segregation with MediatR. Understanding the benefits of separating reads from writes, and how pipeline behaviors provide cross-cutting concerns without coupling.

**Result Pattern:** Moving away from exceptions for flow control to explicit Result<T> types. This makes error handling more predictable and testable, while improving performance by avoiding exception overhead.

**Repository Pattern with Dapper:** Implementing the repository pattern without an ORM. Learning the trade-offs between Dapper's performance and control vs EF Core's convenience. Understanding when lightweight data access is preferable to full-featured ORMs.

**Multi-tenant complexity:** Implementing database-level tenant isolation. Balancing security (automatic filtering) with flexibility (explicit bypass for admin operations). Understanding the trade-offs between different multi-tenancy approaches.

**Testability as a first-class concern:** Designing for testability from the start. All dependencies are interfaces, constructor injection everywhere, pure business logic in handlers. Understanding that testable code is maintainable code.

**Domain modeling:** Translating healthcare workflows into code (appointment state machines, billing calculations, inventory tracking). Understanding that business logic complexity comes from domain rules, not technical patterns.

**Architectural decision-making:** Learning to evaluate trade-offs between different architectural approaches. Understanding that there's no "one size fits all" - the best architecture depends on the specific requirements, team size, and project complexity.

---

## 📚 Documentation

- `README.md` - This file (overview and architecture)
- `PROJECT_STRUCTURE.md` - Detailed folder structure and organization
- `ARCHITECTURE_FINAL_COMPARISON.md` - Comparison with 2025-2026 best practices
- `TESTABILITY_GUIDE.md` - Complete testing guide with examples

---

## License

MIT
