# Clinic Management API

Multi-tenant SaaS platform for healthcare clinic operations built with .NET 10, implementing Vertical Slice Architecture with automatic tenant isolation.

## 🌐 Live Demo

- **API Documentation**: http://clinic-api.runasp.net/scalar/v1
- **Dashboard**: https://clinic-dashboard-ecru.vercel.app
- **Website**: https://clinic-website-lime.vercel.app

## 📦 Repositories

- **Backend API**: https://github.com/SherifOthman/clinic-api
- **Dashboard**: https://github.com/SherifOthman/clinic-dashboard
- **Website**: https://github.com/SherifOthman/clinic-website

---

## Architecture

### Vertical Slice Architecture

**Why VSA over Layered Architecture:**

- Features are independent units containing endpoint → validation → business logic → data access
- Reduces coupling between features - changes to Patient management don't affect Billing
- Eliminates generic repositories and service abstractions that add no value
- Each feature owns its complete vertical stack, making it easier to reason about and test
- Better aligns with how features are developed and deployed in real-world scenarios

**Structure:**

```
Features/
├── Auth/
│   ├── Login.cs              # Endpoint + Request/Response + Handler
│   ├── Register.cs
│   └── GetMe.cs
├── Patients/
│   ├── CreatePatient.cs
│   ├── GetPatients.cs        # Includes pagination, filtering, sorting
│   └── UpdatePatient.cs
└── Invoices/
    ├── CreateInvoice.cs
    └── GetInvoicesByPatient.cs
```

Each feature file contains:

- Endpoint mapping (route, HTTP method, authorization)
- Request/Response DTOs with validation attributes
- Business logic handler
- Database queries specific to that feature

**Trade-offs:**

- More files (53 endpoint files vs ~10 controller files)
- Some code duplication (acceptable for independence)
- Easier to navigate and modify individual features

---

## Multi-Tenancy Implementation

### Data Isolation Strategy

**Global Query Filters (EF Core):**

```csharp
// Automatic filtering applied to all queries
modelBuilder.Entity<Patient>()
    .HasQueryFilter(p => p.ClinicId == _currentUserService.ClinicId);
```

**How it works:**

1. User authenticates → JWT contains `ClinicId` claim
2. `CurrentUserService` extracts `ClinicId` from JWT on each request
3. EF Core automatically appends `WHERE ClinicId = @clinicId` to all queries
4. Developers cannot accidentally query cross-tenant data

**Entities with tenant isolation:**

- Patients, Appointments, Invoices, Payments
- Medicines, Medical Services, Medical Supplies
- Staff (Doctors, Receptionists)

**Bypass mechanism:**

- SuperAdmin role can use `.IgnoreQueryFilters()` for system-wide operations
- Used only in admin endpoints for reporting and management

**Automatic ClinicId assignment:**

```csharp
// In SaveChangesAsync override
if (currentClinicId.HasValue)
{
    foreach (var entry in ChangeTracker.Entries())
    {
        if (entry.State == EntityState.Added)
        {
            var clinicIdProperty = entry.Metadata.FindProperty("ClinicId");
            if (clinicIdProperty != null)
            {
                var currentValue = entry.Property("ClinicId").CurrentValue;
                // Only set if not already set (allows explicit override)
                if (currentValue == null || (currentValue is Guid guid && guid == Guid.Empty))
                {
                    entry.Property("ClinicId").CurrentValue = currentClinicId.Value;
                }
            }
        }
    }
}
```

**Security guarantee:**

- No way to access another clinic's data without explicit `.IgnoreQueryFilters()`
- All queries are tenant-scoped by default
- Database-level isolation (not application-level filtering)

---

## Authentication & Authorization

### JWT Token Flow

**Access Token (60 minutes):**

- Stored in HTTP-only cookie (`accessToken`)
- Contains: UserId, Email, Roles, ClinicId, UserType
- Used for API authentication via `JwtBearerAuthentication`

**Refresh Token (30 days):**

- Stored in HTTP-only cookie (`refreshToken`)
- Stored in database with expiry and revocation support
- Used to obtain new access token without re-login

**Login Flow:**

```
1. POST /auth/login { email, password }
2. Validate credentials (ASP.NET Identity)
3. Check email confirmation status
4. Generate access token + refresh token
5. Set both tokens in HTTP-only cookies
6. Return 204 No Content
7. Frontend calls GET /auth/me to fetch user data
```

**Cookie Configuration:**

- `HttpOnly: true` - Prevents XSS attacks
- `Secure: true` (production) - HTTPS only
- `SameSite: None` (production) / `Lax` (development)
- Automatic expiry handling

**Role-Based Authorization:**

- Roles: SuperAdmin, ClinicOwner, Doctor, Receptionist
- Applied via `[Authorize(Roles = "...")]` or `.RequireAuthorization()`
- ClinicOwner: Full clinic management
- Doctor: Patient records, prescriptions, appointments
- Receptionist: Patient registration, appointment scheduling

**Token Refresh:**

- Automatic via refresh token endpoint
- Old refresh token is revoked on use (rotation)
- Background service cleans expired tokens daily

---

## Database Design

### Entity Overview

**44 Domain Entities across 8 modules:**

**Identity (6 entities):**

- User (base identity with ASP.NET Identity)
- Doctor, Receptionist, ClinicOwner (role-specific data)
- RefreshToken (token management)
- StaffInvitation (onboarding workflow)

**Clinic (5 entities):**

- Clinic, ClinicBranch
- ClinicBranchAppointmentPrice, ClinicBranchPhoneNumber
- DoctorWorkingDay

**Patient (4 entities):**

- Patient, PatientPhone, PatientAllergy, PatientChronicDisease

**Appointment (2 entities):**

- Appointment, AppointmentType

**Billing (3 entities):**

- Invoice, InvoiceItem, Payment

**Inventory (6 entities):**

- Medicine, MedicalService, MedicalSupply
- ClinicMedication, MedicineDispensing, Medication

**Medical Records (15 entities):**

- MedicalVisit, Prescription, PrescriptionItem
- LabTest, LabTestOrder, MedicalVisitLabTest
- RadiologyTest, RadiologyOrder, MedicalVisitRadiology
- MeasurementAttribute, DoctorMeasurementAttribute
- SpecializationMeasurementAttribute, MedicalVisitMeasurement
- MedicalFile, PatientMedicalFile

**Reference Data (3 entities):**

- ChronicDisease, Specialization, SubscriptionPlan

### Relationship Patterns

**One-to-Many:**

- Clinic → Patients (1:N)
- Patient → Appointments (1:N)
- Invoice → InvoiceItems (1:N)
- Invoice → Payments (1:N)

**Many-to-Many:**

- Patient ↔ ChronicDiseases (via PatientChronicDisease)
- Doctor ↔ MeasurementAttributes (via DoctorMeasurementAttribute)

**Soft Delete Implementation:**

```csharp
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
```

**Global query filter excludes soft-deleted:**

```csharp
modelBuilder.Entity<Patient>()
    .HasQueryFilter(p => !p.IsDeleted && p.ClinicId == _currentUserService.ClinicId);
```

**Benefits:**

- Data recovery capability
- Audit trail for compliance
- Referential integrity maintained
- Historical reporting possible

### Query Optimization

**Indexes (36 EF Core configurations):**

- Foreign keys automatically indexed
- Composite indexes on frequently queried columns
- Example: `(ClinicId, PatientCode)` for patient lookup
- Example: `(ClinicId, AppointmentDate)` for appointment queries

**Pagination:**

- All list endpoints support `pageNumber` and `pageSize`
- Default page size: 10, max: 100
- Uses `Skip()` and `Take()` with proper ordering

**Eager Loading:**

- `.Include()` used strategically to avoid N+1 queries
- Example: Loading patient with phone numbers and chronic diseases in single query

---

## Business Logic Complexity

### Appointment State Machine

**States:** Pending → Confirmed → Completed/Cancelled

**Transitions:**

- Pending → Confirmed: Receptionist/Doctor confirms
- Confirmed → Completed: Doctor marks as done
- Any → Cancelled: Can be cancelled with reason

**Validation:**

- Cannot confirm already confirmed appointment
- Cannot complete non-confirmed appointment
- Cancelled appointments cannot transition to other states

### Billing Calculations

**Invoice calculation logic:**

```
SubtotalAmount = Sum(InvoiceItems.LineTotal)
FinalAmount = SubtotalAmount - Discount + TaxAmount
TotalPaid = Sum(Payments where Status = Paid)
RemainingAmount = FinalAmount - TotalPaid
IsFullyPaid = RemainingAmount <= 0
IsOverdue = DueDate < Now && !IsFullyPaid && Status != Cancelled
```

**Calculated properties on Invoice entity:**

- `SubtotalAmount`, `FinalAmount`, `TotalPaid`, `RemainingAmount`
- `IsFullyPaid`, `IsOverdue`, `IsPartiallyPaid`
- `DiscountPercentage`, `DaysOverdue`

**Payment tracking:**

- Multiple partial payments supported
- Payment status: Pending, Paid, Failed, Refunded
- Payment methods: Cash, Card, Insurance, BankTransfer

### Inventory Management

**Medicine stock tracking:**

- Box/strip system (e.g., 1 box = 10 strips)
- `TotalStripsInStock`, `FullBoxesInStock`, `RemainingStrips`
- Expiry date monitoring
- Discontinued flag for obsolete medicines

**Stock operations:**

- Add stock: Increases `TotalStripsInStock`
- Remove stock: Decreases with validation (cannot go negative)
- Stock movement tracking with reason and timestamp

**Low stock alerts:**

- Calculated property: `IsLowStock` based on threshold
- Used for inventory management dashboard

---

## Error Handling

### RFC 7807 Problem Details

**Standardized error response:**

```json
{
  "code": "INSUFFICIENT_STOCK",
  "title": "Insufficient Stock",
  "status": 400,
  "detail": "Available: 5, Requested: 10",
  "data": { "available": 5, "requested": 10 },
  "traceId": "0HMVFE..."
}
```

**Error code categories:**

- Validation: `VALIDATION_ERROR`, `REQUIRED_FIELD`, `INVALID_FORMAT`
- Authentication: `INVALID_CREDENTIALS`, `EMAIL_NOT_CONFIRMED`, `TOKEN_EXPIRED`
- Authorization: `ACCESS_DENIED`, `INSUFFICIENT_PERMISSIONS`
- Business Logic: `INSUFFICIENT_STOCK`, `APPOINTMENT_CONFLICT`, `INVOICE_ALREADY_PAID`
- System: `INTERNAL_ERROR`, `DATABASE_ERROR`, `EXTERNAL_SERVICE_ERROR`

**40+ predefined error codes** for frontend i18n translation.

### Global Exception Middleware

**Catches unhandled exceptions:**

```csharp
try { await _next(context); }
catch (Exception ex)
{
    _logger.LogError(ex, "Unhandled exception");
    return ProblemDetails with appropriate status code;
}
```

**Exception mapping:**

- `UnauthorizedAccessException` → 403
- `KeyNotFoundException` → 404
- `InvalidOperationException` → 400
- `ArgumentException` → 400
- All others → 500

**Benefits:**

- Consistent error format across all endpoints
- Frontend can translate error codes to user's language
- Trace IDs for debugging
- No sensitive information leaked in production

---

## Logging & Observability

### Serilog Configuration

**Structured logging:**

```csharp
_logger.LogInformation(
    "Patient created: {PatientId} {PatientCode} by {UserId} in {ClinicId}",
    patient.Id, patient.PatientCode, currentUser.UserId, currentUser.ClinicId
);
```

**Log levels by environment:**

- Development: Debug level, console + file
- Production: Warning level, file only (3-day retention)

**Logged events:**

- Authentication attempts (success/failure)
- Entity creation/modification/deletion
- Business rule violations
- External service calls (GeoNames, SMTP)
- Performance issues (slow queries)

**Log sinks:**

- Console (development)
- Rolling file (daily rotation, 7-day retention in dev, 3-day in production)
- Structured format for log aggregation tools

**Trace IDs:**

- Included in all error responses
- Correlates frontend errors with backend logs
- Essential for debugging production issues

---

## API Design

### REST Semantics

**HTTP Methods:**

- GET: Retrieve resources (idempotent)
- POST: Create resources
- PUT: Update entire resource
- DELETE: Soft delete resource

**Status Codes:**

- 200 OK: Successful GET/PUT
- 201 Created: Successful POST (with Location header)
- 204 No Content: Successful operation with no response body
- 400 Bad Request: Validation errors
- 401 Unauthorized: Missing/invalid authentication
- 403 Forbidden: Insufficient permissions
- 404 Not Found: Resource doesn't exist
- 500 Internal Server Error: Unhandled exceptions

**Endpoint naming:**

- Plural nouns: `/patients`, `/appointments`, `/invoices`
- Resource IDs in path: `/patients/{id}`
- Actions as verbs: `/appointments/{id}/confirm`
- Query parameters for filtering: `/patients?searchTerm=john&gender=Male`

### DTO Separation

**Request DTOs:**

- Validation attributes (`[Required]`, `[EmailAddress]`, `[Range]`)
- No navigation properties
- Flat structure for simple binding

**Response DTOs:**

- Calculated fields included
- Related data via projections (not full entities)
- Consistent naming with frontend types

**Benefits:**

- API contract independent of database schema
- Can change database without breaking API
- Prevents over-posting attacks
- Clear separation of concerns

### Validation Strategy

**Data Annotations:**

```csharp
public record Request(
    [Required]
    [MaxLength(200)]
    string FullName,

    [Required]
    [EnumDataType(typeof(Gender))]
    Gender Gender,

    [Required]
    [CustomValidation(typeof(CustomValidators), nameof(CustomValidators.MustBeInPast))]
    DateTime DateOfBirth
);
```

**ASP.NET Core automatic validation:**

- Validates before endpoint handler executes
- Returns 400 with validation errors automatically
- No manual validation code in endpoints

**Custom validators:**

- `MustBeInPast` for date of birth
- `MustBeInFuture` for appointment dates
- Phone number validation via libphonenumber

---

## Security

### Authentication Security

- JWT tokens in HTTP-only cookies (not localStorage)
- Refresh token rotation (old token revoked on use)
- Email confirmation required before login
- Password hashing via ASP.NET Identity (PBKDF2)
- Token expiry: 60 minutes (access), 30 days (refresh)

### Authorization Security

- Role-based access control on all endpoints
- Multi-tenant data isolation at database level
- No way to access another clinic's data without explicit bypass
- SuperAdmin role for system-wide operations only

### Input Validation

- All inputs validated via data annotations
- SQL injection prevented by parameterized queries (EF Core)
- XSS prevented by not returning HTML
- CORS configured for specific origins only

### Configuration Security

- Secrets in environment variables (not in code)
- Different configurations per environment
- JWT signing key minimum 32 characters
- Database connection strings secured

---

## Production Readiness

### Database Migrations

**3 migrations applied:**

1. `InitialCreate` - All 44 entities with relationships
2. `FixInvoiceAppointmentRelationship` - Corrected FK constraints
3. `RemoveClinicIdFromUser` - Schema evolution

**Migration strategy:**

- Automatic on startup in development
- Manual in production (via deployment pipeline)
- Rollback capability via `dotnet ef database update <migration>`

### Configuration Management

**Environment-specific settings:**

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Dev overrides
- `appsettings.Production.json` - Production overrides

**Configuration sections:**

- ConnectionStrings (SQL Server)
- JWT (signing key, expiry)
- CORS (allowed origins)
- SMTP (email service)
- Cookie (security settings)
- GeoNames (API credentials)
- FileStorage (upload paths)
- Serilog (logging levels)

### Deployment

**Hosting:**

- Deployed on ASP.NET hosting service
- SQL Server database (Azure SQL compatible)
- Automatic HTTPS redirection
- Health check endpoint

**Environment separation:**

- Development: localhost, relaxed CORS, debug logging
- Production: HTTPS only, strict CORS, warning logging

### Background Services

**RefreshTokenCleanupService:**

- Runs daily
- Removes expired refresh tokens
- Prevents database bloat
- Implemented as `IHostedService`

---

## Technology Stack

- **.NET 10** - Latest framework with minimal APIs
- **Entity Framework Core 10** - ORM with 36 entity configurations
- **ASP.NET Core Identity** - User management and password hashing
- **JWT Bearer Authentication** - Token-based auth
- **Scalar.AspNetCore** - Interactive API documentation
- **Serilog** - Structured logging
- **libphonenumber** - International phone validation
- **MailKit** - SMTP email service
- **GeoNames API** - Location data (countries, states, cities)

---

## Project Metrics

- **15 Feature Areas** - Auth, Patients, Appointments, Invoices, Payments, Medicines, etc.
- **53 API Endpoints** - Complete CRUD operations
- **44 Domain Entities** - Comprehensive data model
- **36 EF Core Configurations** - Explicit relationship mapping
- **18 Infrastructure Services** - Cross-cutting concerns
- **15 Domain Enums** - Type-safe enumerations
- **40+ Error Codes** - Standardized error handling
- **2 Custom Middleware** - Global exception handling, JWT cookie processing
- **3 Database Migrations** - Schema evolution tracking

---

## License

MIT
