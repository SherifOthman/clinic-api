# Clinic Management API

## 🚀 Overview

Multi-tenant SaaS backend for clinic operations built with .NET 10 and Vertical Slice Architecture.

The system handles patient management, appointment scheduling, billing, inventory tracking, and medical records. Each clinic operates as an isolated tenant with automatic data scoping at the database level. Authentication uses JWT tokens with refresh token rotation, and authorization is role-based (ClinicOwner, Doctor, Receptionist).

The architecture organizes code by feature rather than technical layer - each feature contains its endpoint, validation, business logic, and data access in a single file. This reduces coupling between features and makes the codebase easier to navigate.

Core modules include authentication, patient records, appointments, invoicing with payment tracking, medicine inventory with expiration monitoring, and medical visit documentation with prescriptions and lab orders.

**Live Demo**: http://clinic-api.runasp.net/scalar/v1  
**Dashboard**: https://clinic-dashboard-ecru.vercel.app  
**Website**: https://clinic-website-lime.vercel.app

**Repositories**: [API](https://github.com/SherifOthman/clinic-api) • [Dashboard](https://github.com/SherifOthman/clinic-dashboard) • [Website](https://github.com/SherifOthman/clinic-website)

---

## 🏗 Architecture

**Refactored from Clean Architecture to Vertical Slice Architecture.** The original implementation used Controllers + MediatR + Repository Pattern + Unit of Work. This was removed because most operations are simple CRUD that don't benefit from these abstractions.

**What was removed:**

- Controllers that forwarded requests to MediatR handlers
- Command/Query handlers that just called repository methods
- Repository pattern wrapping EF Core (which is already a repository)
- Unit of Work pattern (DbContext already handles transactions)
- Rich domain models with business logic in entities

**Result:** Eliminated ~1,900 lines of code while maintaining functionality.

**Current structure:** Each feature is one file containing endpoint, validation, business logic, and data access.

```
Features/Patients/
├── CreatePatient.cs      # POST /patients - complete operation
├── GetPatients.cs        # GET /patients - with filtering/pagination
└── UpdatePatient.cs      # PUT /patients/{id} - complete operation
```

**Why this works:** Most operations are straightforward CRUD. Complex logic (invoice calculations, medicine stock management) stays in feature handlers where it's actually used, not abstracted into domain models.

**Trade-off:** More files (53 vs ~15 controllers), some duplication. Acceptable for feature independence.

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

**46 entities** organized across 8 domain modules: Identity (6), Clinic (5), Staff (2), Patient (4), Appointment (2), Billing (3), Inventory (6), Medical Records (15), Reference Data (3).

**Key relationships:**

- One-to-Many: Clinic → Staff, Staff → DoctorProfile, Clinic → Patients, Patient → Appointments, Invoice → InvoiceItems, Invoice → Payments
- Many-to-Many: Patient ↔ ChronicDiseases, DoctorProfile ↔ MeasurementAttributes

**Staff architecture:** Staff table links Users to Clinics with role-based membership. DoctorProfile extends Staff with doctor-specific data (only created for users with Doctor role). This separates identity (User) from clinic membership (Staff) from role-specific data (DoctorProfile).

**Soft delete:** All entities inherit from `AuditableEntity` with `IsDeleted`, `DeletedAt`, `DeletedBy` fields. Global query filters automatically exclude soft-deleted records. This maintains referential integrity and enables data recovery.

**Audit tracking:** `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` automatically populated via `SaveChangesAsync` override.

**Query optimization:**

- Foreign keys indexed automatically by EF Core
- Composite indexes on frequently queried columns (e.g., `ClinicId + PatientCode`)
- Pagination on all list endpoints (default 10, max 100)
- Strategic use of `.Include()` to avoid N+1 queries

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

---

## 📦 API Design

**REST semantics:** GET (retrieve), POST (create), PUT (update), DELETE (soft delete). Status codes follow HTTP standards (200, 201, 204, 400, 401, 403, 404, 500).

**Endpoint naming:** Plural nouns (`/patients`, `/appointments`), resource IDs in path (`/patients/{id}`), actions as verbs (`/appointments/{id}/confirm`), query parameters for filtering.

**DTO separation:** Request DTOs have validation attributes and flat structure. Response DTOs include calculated fields and related data via projections. This decouples API contract from database schema and prevents over-posting.

**Validation:** Data annotations (`[Required]`, `[EmailAddress]`, `[Range]`) with ASP.NET Core automatic validation. Returns 400 with validation errors before endpoint handler executes. Custom validators for business rules (date ranges, phone numbers).

---

## 📊 Project Scope

**53 API endpoints** across 15 feature areas: Authentication, Patients, Appointments, Invoices, Payments, Medicines, Medical Services, Medical Supplies, Locations, Measurements, Chronic Diseases, Specializations, Subscription Plans, Onboarding.

**Technology stack:** .NET 10, Entity Framework Core 10, ASP.NET Core Identity, JWT Bearer Authentication, Serilog, Scalar (API docs), MailKit (SMTP), GeoNames API.

**Infrastructure:** 44 domain entities, 36 EF Core configurations, 18 cross-cutting services, 2 custom middleware, 4 database migrations, background service for token cleanup.

---

## 🧠 What I Learned

**Architectural thinking:** Understanding when to choose Vertical Slice over traditional layered architecture. Recognizing that reducing coupling between features is more valuable than eliminating code duplication.

**Multi-tenant complexity:** Implementing database-level tenant isolation using EF Core global query filters. Balancing security (automatic filtering) with flexibility (explicit bypass for admin operations). Understanding the trade-offs between different multi-tenancy approaches.

**Backend system structure:** Organizing a real-world system with multiple interconnected domains (patients, appointments, billing, inventory). Handling cross-cutting concerns (authentication, logging, error handling) without creating tight coupling. Making pragmatic decisions about where to apply patterns and where to keep things simple.

**Domain modeling:** Translating healthcare workflows into code (appointment state machines, billing calculations, inventory tracking). Understanding that business logic complexity comes from domain rules, not technical patterns.

---

## License

MIT
