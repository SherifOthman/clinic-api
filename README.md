# Clinic Management API

A comprehensive clinic management system built with .NET 10, featuring **Vertical Slice Architecture**, **Domain-Driven Design**, and **Multi-Tenancy**.

## Architecture

This project follows **Vertical Slice Architecture** combined with **Domain-Driven Design** principles, organizing code by features rather than technical layers. Each feature is a self-contained vertical slice with everything it needs.

### Core Architectural Patterns

1. **Vertical Slice Architecture (VSA)**: Features are organized as independent slices
2. **Domain-Driven Design (DDD)**: Rich domain models with business logic encapsulation
3. **Multi-Tenancy**: Clinic-based data isolation using global query filters
4. **CQRS-lite**: Separation of commands and queries within features
5. **Repository Pattern**: Abstracted data access through EF Core
6. **Specification Pattern**: Reusable query logic

### Project Structure

```
ClinicManagement.API/              # Single project - VSA style
├── Features/                      # Vertical slices (15 feature areas, 53 endpoints)
│   ├── Appointments/              # Appointment scheduling & management
│   │   ├── CreateAppointment.cs
│   │   ├── GetAppointments.cs
│   │   ├── ConfirmAppointment.cs
│   │   ├── CompleteAppointment.cs
│   │   └── CancelAppointment.cs
│   ├── Auth/                      # Authentication & authorization
│   │   ├── Register.cs
│   │   ├── Login.cs
│   │   ├── Logout.cs
│   │   ├── ChangePassword.cs
│   │   ├── ForgotPassword.cs
│   │   ├── ResetPassword.cs
│   │   ├── CheckEmailAvailability.cs
│   │   └── CheckUsernameAvailability.cs
│   ├── Patients/                  # Patient management
│   ├── Invoices/                  # Billing & invoicing
│   ├── Payments/                  # Payment processing
│   ├── Medicines/                 # Medicine inventory
│   ├── MedicalServices/           # Medical services catalog
│   ├── MedicalSupplies/           # Medical supplies inventory
│   ├── Locations/                 # Location data (countries, states, cities)
│   ├── Specializations/           # Medical specializations
│   ├── ChronicDiseases/           # Chronic disease reference data
│   ├── PatientChronicDiseases/    # Patient chronic disease associations
│   ├── Measurements/              # Measurement attributes
│   ├── SubscriptionPlans/         # Subscription plan management
│   └── Onboarding/                # Clinic onboarding flow
├── Entities/                      # Domain entities (43 entities)
│   ├── Appointment/               # Appointment aggregate
│   ├── Billing/                   # Invoice, InvoiceItem, Payment
│   ├── Clinic/                    # Clinic, ClinicBranch, DoctorWorkingDay
│   ├── Identity/                  # User, Doctor, Receptionist, RefreshToken
│   ├── Inventory/                 # Medicine, MedicalService, MedicalSupply
│   ├── Medical/                   # MedicalVisit, Prescription, LabTest, Radiology
│   ├── Patient/                   # Patient, PatientAllergy, PatientChronicDisease
│   └── Reference/                 # ChronicDisease, Specialization, SubscriptionPlan
├── Common/                        # Shared domain code
│   ├── BaseEntity.cs              # Base entity with GUID ID
│   ├── AuditableEntity.cs         # Soft delete & audit fields
│   ├── Constants/                 # Error codes, roles, validation rules
│   ├── Enums/                     # Domain enums (15 enums)
│   ├── Exceptions/                # Domain exceptions (12 exception types)
│   ├── Extensions/                # Extension methods
│   ├── Models/                    # API models (ProblemDetails, PaginatedResult)
│   ├── Options/                   # Configuration options
│   └── Validation/                # Custom validators
├── Infrastructure/                # Infrastructure layer
│   ├── Data/                      # EF Core context, configurations, migrations
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/        # Entity configurations (35 files)
│   │   └── Migrations/
│   ├── Services/                  # Infrastructure services (18 services)
│   │   ├── AuthenticationService.cs
│   │   ├── TokenService.cs
│   │   ├── RefreshTokenService.cs
│   │   ├── EmailConfirmationService.cs
│   │   ├── GeoNamesService.cs     # Location data integration
│   │   ├── PhoneValidationService.cs
│   │   └── ...
│   └── Middleware/                # HTTP middleware
│       ├── GlobalExceptionMiddleware.cs
│       └── JwtCookieMiddleware.cs
├── Endpoints.cs                   # Auto-discovery endpoint registration
├── DependencyInjection.cs         # Service registration
└── Program.cs                     # Application entry point
```

### Key Architectural Principles

1. **Feature-Centric Organization**: Code is organized around business features, not technical layers
2. **Self-Contained Slices**: Each feature contains everything it needs (endpoint, validation, business logic)
3. **Rich Domain Models**: Entities encapsulate business logic and enforce invariants
4. **Multi-Tenancy by Design**: Automatic clinic-based data isolation via global query filters
5. **Soft Delete Pattern**: Entities are marked as deleted, not physically removed
6. **Aggregate Roots**: Domain entities manage their own consistency boundaries
7. **Factory Methods**: Entities use static factory methods to ensure valid creation
8. **Minimal Coupling Between Slices**: Features are independent and don't depend on each other
9. **Pragmatic Sharing**: Shared code (entities, services) is kept in common folders when it makes sense

### Domain-Driven Design Patterns

#### Aggregates

- **Appointment**: Manages appointment lifecycle, state transitions, and business rules
- **Invoice**: Manages invoice items, payments, discounts, and payment state
- **Patient**: Manages patient information, phone numbers, allergies, and chronic diseases
- **Medicine**: Manages inventory in boxes/strips with expiry tracking and stock validation

#### Value Objects

- Money (decimal with precision)
- Email (validated format)
- PhoneNumber (validated with libphonenumber)

#### Domain Events

- Appointment state changes
- Invoice payment received
- Patient allergy added (critical for safety)

#### Specifications

- Reusable query logic for filtering and searching

### Multi-Tenancy Architecture

The system implements **database-per-tenant isolation** using EF Core global query filters:

- **ClinicId Claim**: JWT tokens include ClinicId for automatic filtering
- **Global Query Filters**: All queries automatically filter by current user's clinic
- **Automatic ClinicId Assignment**: New entities automatically get ClinicId from current user
- **SuperAdmin Override**: SuperAdmin users can bypass filters to see all clinics

```csharp
// Automatic filtering - no manual ClinicId checks needed
var patients = await db.Patients.ToListAsync(); // Only returns current clinic's patients

// SuperAdmin can see all
var allPatients = await db.Patients.IgnoreQueryFilters().ToListAsync();
```

## Features

### Core Features

- ✅ **Authentication & Authorization**
  - JWT + Refresh Token authentication
  - Cookie-based token storage
  - Token rotation for security
  - Role-based access control (SuperAdmin, ClinicOwner, Doctor, Receptionist)
  - Email confirmation
  - Password reset flow
  - Real-time email/username availability checking

- ✅ **Multi-Tenancy**
  - Clinic-based data isolation
  - Automatic ClinicId filtering
  - SuperAdmin can access all clinics
  - Soft delete with audit trail

- ✅ **Patient Management**
  - Patient registration with demographics
  - Multiple phone numbers per patient
  - Chronic disease tracking
  - Allergy management (critical for prescription safety)
  - Age calculations and categorization
  - Emergency contact information

- ✅ **Appointment Scheduling**
  - Appointment booking with queue management
  - Doctor availability tracking
  - Appointment state machine (Pending → Confirmed → Completed/Cancelled)
  - Conflict detection
  - Appointment type pricing

- ✅ **Medicine Inventory**
  - Box and strip-based inventory
  - Expiry date tracking
  - Low stock alerts
  - Reorder level management
  - Stock movement tracking
  - Discontinued medicine handling

- ✅ **Billing & Invoicing**
  - Flexible invoice items (services, medicines, supplies, lab tests, radiology)
  - Discount management (amount or percentage)
  - Tax calculation
  - Invoice state machine (Draft → Issued → PartiallyPaid → FullyPaid)
  - Overdue tracking

- ✅ **Payment Processing**
  - Multiple payment methods (Cash, Card, Insurance, BankTransfer)
  - Partial payment support
  - Payment history tracking
  - Automatic invoice status updates

- ✅ **Medical Services & Supplies**
  - Service catalog management
  - Supply inventory tracking
  - Pricing management

- ✅ **Location Management**
  - GeoNames integration for countries, states, cities
  - Bilingual support (Arabic/English)
  - 24-hour caching for performance

- ✅ **Reference Data**
  - Chronic diseases catalog
  - Medical specializations
  - Subscription plans
  - Measurement attributes

- ✅ **Onboarding Flow**
  - Clinic registration
  - Branch setup
  - Subscription plan selection
  - Automatic user-clinic linking

### Technical Features

- ✅ **Error Handling**
  - RFC 7807 Problem Details standard
  - Standardized error codes for i18n
  - Field-level validation errors
  - Global exception middleware

- ✅ **API Documentation**
  - Swagger/OpenAPI integration
  - JWT authentication in Swagger UI
  - Comprehensive endpoint documentation

- ✅ **Caching**
  - Output caching for reference data (1 hour)
  - Location data caching (24 hours)
  - Memory cache for frequently accessed data

- ✅ **Logging**
  - Serilog structured logging
  - File and console sinks
  - Request/response logging
  - Error tracking with trace IDs

- ✅ **Background Jobs**
  - Refresh token cleanup
  - Expired data cleanup
  - Email sending queue

- ✅ **Validation**
  - .NET 10 native validation
  - Custom validators for business rules
  - Phone number validation (libphonenumber)
  - Email format validation

## Technology Stack

### Core Framework

- **.NET 10.0** - Latest .NET framework
- **ASP.NET Core Web API** - Minimal APIs with endpoint routing
- **C# 13** - Latest language features

### Database & ORM

- **Entity Framework Core 10.0** - ORM with LINQ support
- **SQL Server** - Primary database
- **EF Core Migrations** - Database versioning

### Authentication & Security

- **ASP.NET Core Identity** - User management
- **JWT Bearer Authentication** - Stateless authentication
- **System.IdentityModel.Tokens.Jwt** - Token generation/validation
- **Cookie-based token storage** - Secure token handling

### API Documentation

- **Swashbuckle.AspNetCore 7.2.1** - Swagger/OpenAPI integration

### Logging

- **Serilog 8.0** - Structured logging
- **Serilog.Sinks.File** - File logging
- **Serilog.Sinks.Console** - Console logging

### Background Jobs

- **Hangfire 1.8** - Background job processing
- **Hangfire.SqlServer** - SQL Server storage for Hangfire

### External Integrations

- **libphonenumber-csharp 9.0** - Phone number validation
- **MailKit 4.14** - Email sending (SMTP)
- **GeoNames API** - Location data (countries, states, cities)
- **Stripe.net 49.2** - Payment processing (ready for integration)

### Caching & Performance

- **Memory Cache** - In-memory caching
- **Output Cache** - HTTP response caching
- **EF Core Query Filters** - Automatic multi-tenancy filtering

## Getting Started

### Prerequisites

- **.NET 10.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **SQL Server** - SQL Server 2019+ or SQL Server Express
- **Visual Studio 2022** or **VS Code** (optional)

### Configuration

1. **Update Connection String**

Edit `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClinicManagement;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

2. **Configure JWT Settings**

```json
{
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long",
    "Issuer": "ClinicManagementAPI",
    "Audience": "ClinicManagementClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

3. **Configure SMTP (Optional)**

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@clinic.com",
    "FromName": "Clinic Management"
  }
}
```

4. **Configure GeoNames (Optional)**

```json
{
  "GeoNames": {
    "Username": "your-geonames-username"
  }
}
```

### Installation

1. **Clone the repository**

```bash
git clone <repository-url>
cd clinic-api
```

2. **Restore packages**

```bash
dotnet restore
```

3. **Build the project**

```bash
dotnet build
```

4. **Run the application**

```bash
dotnet run --project ClinicManagement.API
```

The API will be available at:

- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

### Database Setup

The database is **automatically initialized** on first run:

- Migrations are applied
- Seed data is loaded (roles, subscription plans, chronic diseases, specializations)
- Default admin user is created (if configured)

To manually manage migrations:

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project ClinicManagement.API

# Update database
dotnet ef database update --project ClinicManagement.API

# Remove last migration
dotnet ef migrations remove --project ClinicManagement.API
```

## API Documentation

### Swagger UI

Interactive API documentation is available at `/swagger` when running the application.

Features:

- Browse all endpoints
- Test endpoints directly from the browser
- JWT authentication support (click "Authorize" button)
- Request/response schemas
- Error code documentation

### Authentication

The API uses JWT Bearer authentication with refresh tokens:

1. **Register** a new user: `POST /api/auth/register`
2. **Login** to get tokens: `POST /api/auth/login`
3. **Access protected endpoints** using the access token
4. **Refresh** expired tokens: `POST /api/auth/refresh`

Tokens are stored in HTTP-only cookies for security.

### Error Handling

All errors follow **RFC 7807 Problem Details** standard:

```json
{
  "code": "INSUFFICIENT_STOCK",
  "title": "Insufficient Stock",
  "status": 400,
  "detail": "Available: 5, Requested: 10",
  "data": {
    "available": 5,
    "requested": 10
  },
  "errors": {
    "email": ["INVALID_EMAIL"]
  },
  "traceId": "0HMVFE..."
}
```

Error codes are designed for frontend i18n translation (Arabic/English).

### Key Endpoints

#### Authentication

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get tokens
- `POST /api/auth/logout` - Logout and revoke tokens
- `GET /api/auth/me` - Get current user info
- `GET /api/auth/check-email` - Check email availability
- `GET /api/auth/check-username` - Check username availability

#### Patients

- `GET /api/patients` - List patients (paginated, searchable)
- `POST /api/patients` - Create patient
- `GET /api/patients/{id}` - Get patient details
- `PUT /api/patients/{id}` - Update patient
- `DELETE /api/patients/{id}` - Soft delete patient

#### Appointments

- `GET /api/appointments` - List appointments
- `POST /api/appointments` - Create appointment
- `POST /api/appointments/{id}/confirm` - Confirm appointment
- `POST /api/appointments/{id}/complete` - Complete appointment
- `POST /api/appointments/{id}/cancel` - Cancel appointment

#### Invoices & Payments

- `GET /api/invoices` - List invoices
- `POST /api/invoices` - Create invoice
- `GET /api/invoices/{id}` - Get invoice details
- `POST /api/invoices/{id}/payments` - Record payment
- `POST /api/invoices/{id}/cancel` - Cancel invoice

#### Medicines

- `GET /api/medicines` - List medicines
- `POST /api/medicines` - Create medicine
- `POST /api/medicines/{id}/stock/add` - Add stock
- `POST /api/medicines/{id}/stock/remove` - Remove stock

#### Locations

- `GET /api/locations/countries` - Get countries (cached 24h)
- `GET /api/locations/states?countryId={id}` - Get states
- `GET /api/locations/cities?stateId={id}` - Get cities

## Domain Model

### Core Aggregates

#### Appointment

- Manages appointment lifecycle and state transitions
- Enforces business rules (no past dates, conflict detection)
- Links to patient, doctor, clinic branch, and invoice
- State machine: Pending → Confirmed → Completed/Cancelled

#### Invoice

- Manages invoice items and payments
- Supports multiple item types (services, medicines, supplies, lab tests, radiology)
- Discount and tax calculation
- State machine: Draft → Issued → PartiallyPaid → FullyPaid → Cancelled
- Tracks overdue invoices

#### Patient

- Manages patient demographics and medical information
- Multiple phone numbers with primary designation
- Allergy tracking (critical for prescription safety)
- Chronic disease associations
- Emergency contact information
- Age calculations and categorization (Adult, Minor, Senior)

#### Medicine

- Box and strip-based inventory management
- Expiry date tracking and alerts
- Stock level management (minimum, reorder levels)
- Price calculation for boxes and strips
- Discontinued medicine handling
- Inventory value calculation

### Entity Relationships

```
Clinic (1) ──→ (N) ClinicBranch
  │                    │
  │                    ├──→ (N) Medicine
  │                    ├──→ (N) MedicalService
  │                    ├──→ (N) MedicalSupply
  │                    └──→ (N) DoctorWorkingDay
  │
  ├──→ (N) Patient
  │         │
  │         ├──→ (N) PatientPhone
  │         ├──→ (N) PatientAllergy
  │         ├──→ (N) PatientChronicDisease
  │         └──→ (N) Appointment
  │
  ├──→ (N) Invoice
  │         │
  │         ├──→ (N) InvoiceItem
  │         └──→ (N) Payment
  │
  └──→ (N) User (Doctor, Receptionist, ClinicOwner)
            │
            └──→ (N) RefreshToken
```

### Soft Delete & Audit Trail

All entities inherit from `AuditableEntity` which provides:

- `CreatedAt`, `CreatedBy` - Creation tracking
- `UpdatedAt`, `UpdatedBy` - Modification tracking
- `DeletedAt`, `DeletedBy` - Soft delete tracking
- `IsDeleted` - Soft delete flag

Soft-deleted records are automatically excluded from queries via global filters.

## Development

### Project Statistics

- **15 Feature Areas** - Organized by business capability
- **53 Endpoints** - RESTful API endpoints
- **43 Domain Entities** - Rich domain models
- **35 Entity Configurations** - EF Core mappings
- **18 Infrastructure Services** - Cross-cutting concerns
- **12 Domain Exceptions** - Business rule violations
- **15 Domain Enums** - Type-safe enumerations

### Code Organization Principles

1. **Feature Folders**: Each feature is self-contained
2. **Minimal APIs**: Using .NET minimal API pattern
3. **Endpoint Discovery**: Automatic endpoint registration via reflection
4. **Domain Logic in Entities**: Business rules live in domain models
5. **Factory Methods**: Entities use static factory methods for creation
6. **Extension Methods**: Reusable query and validation logic

### Adding a New Feature

1. Create a new folder in `Features/`
2. Create endpoint files (e.g., `CreatePatient.cs`)
3. Implement `IEndpoint` interface
4. Define Request/Response records
5. Implement business logic
6. Endpoints are automatically discovered and registered

Example:

```csharp
public class CreatePatientEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/patients", HandleAsync)
            .RequireAuthorization()
            .WithName("CreatePatient")
            .WithTags("Patients");
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        // Use domain factory method
        var patient = Patient.Create(
            patientCode: await GeneratePatientCode(db, ct),
            clinicId: currentUser.ClinicId!.Value,
            fullName: request.FullName,
            gender: request.Gender,
            dateOfBirth: request.DateOfBirth
        );

        db.Patients.Add(patient);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/patients/{patient.Id}",
            new Response(patient.Id, patient.FullName));
    }

    public record Request(string FullName, Gender Gender, DateTime DateOfBirth);
    public record Response(Guid Id, string FullName);
}
```

## Architecture References

- [Vertical Slice Architecture by Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
- [Structuring Vertical Slices by Milan Jovanović](https://www.milanjovanovic.tech/blog/vertical-slice-architecture-structuring-vertical-slices)
- [Vertical Slice Architecture by Julio Casal](https://juliocasal.com/blog/vertical-slice-architecture)

## License

MIT
