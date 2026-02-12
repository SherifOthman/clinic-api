# Clinic Management API

A comprehensive clinic management system built with .NET 10, featuring **Vertical Slice Architecture** and **Multi-Tenancy**.

## 🌐 Live Demo

- **API Documentation**: http://clinic-api.runasp.net/scalar/v1

## Project Structure

```
clinic-api/
├── src/
│   └── ClinicManagement.API/          # Main API project
│       ├── Features/                   # 15 feature areas, 53 endpoints
│       ├── Entities/                   # 44 domain entities
│       ├── Infrastructure/             # Data access, services, middleware
│       └── Common/                     # Shared utilities, exceptions, enums
├── tests/
│   ├── ClinicManagement.Tests/        # Unit tests
│   │   ├── Domain/                     # Entity business logic tests
│   │   ├── Services/                   # Service layer tests
│   │   ├── Extensions/                 # Extension method tests
│   │   └── Validation/                 # Validation tests
│   └── ClinicManagement.IntegrationTests/  # Integration tests
│       ├── Auth/                       # Authentication flow tests
│       └── Helpers/                    # Test utilities
└── ClinicManagement.sln
```

## Architecture

### Core Patterns

1. **Vertical Slice Architecture**: Features organized as independent slices
2. **Multi-Tenancy**: Automatic clinic-based data isolation
3. **Soft Delete**: Audit trail with soft delete pattern

### Feature Areas

```
src/ClinicManagement.API/Features/
├── Appointments/                  # 6 endpoints
├── Auth/                          # 15 endpoints
├── Patients/                      # 5 endpoints
├── Invoices/                      # 4 endpoints
├── Payments/                      # 2 endpoints
├── Medicines/                     # 5 endpoints
├── MedicalServices/               # 2 endpoints
├── MedicalSupplies/               # 2 endpoints
├── Locations/                     # 3 endpoints (bilingual)
├── ChronicDiseases/               # 2 endpoints
├── PatientChronicDiseases/        # 2 endpoints
├── Measurements/                  # 1 endpoint
├── Specializations/               # 2 endpoints
├── SubscriptionPlans/             # 1 endpoint
└── Onboarding/                    # 1 endpoint
```

### Domain Entities

```
src/ClinicManagement.API/Entities/
├── Appointment/                   # Appointment, AppointmentType
├── Billing/                       # Invoice, InvoiceItem, Payment
├── Clinic/                        # Clinic, ClinicBranch, DoctorWorkingDay
├── Identity/                      # User, Doctor, Receptionist, RefreshToken
├── Inventory/                     # Medicine, MedicalService, MedicalSupply
├── Medical/                       # MedicalVisit, Prescription, LabTest, etc.
├── Patient/                       # Patient, PatientAllergy, PatientChronicDisease
└── Reference/                     # ChronicDisease, Specialization, SubscriptionPlan
```

### Infrastructure

```
src/ClinicManagement.API/Infrastructure/
├── Data/                          # EF Core, 36 configurations
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   └── Migrations/
├── Services/                      # 18 services
│   ├── AuthenticationService.cs
│   ├── TokenService.cs
│   ├── RefreshTokenService.cs
│   ├── EmailConfirmationService.cs
│   ├── GeoNamesService.cs
│   └── ...
└── Middleware/                    # 2 middleware
    ├── GlobalExceptionMiddleware.cs
    └── JwtCookieMiddleware.cs
```

### Multi-Tenancy

Automatic clinic-based data isolation using EF Core global query filters:

```csharp
// Automatic filtering by ClinicId from JWT
var patients = await db.Patients.ToListAsync(); // Only current clinic's data

// SuperAdmin can bypass filters
var allPatients = await db.Patients.IgnoreQueryFilters().ToListAsync();
```

## Features

- ✅ **Authentication**: JWT + Refresh Tokens, email confirmation, password reset
- ✅ **Multi-Tenancy**: Automatic clinic-based data isolation
- ✅ **Patients**: Demographics, allergies, chronic diseases, multiple phones
- ✅ **Appointments**: Scheduling, state machine, conflict detection
- ✅ **Medicines**: Box/strip inventory, expiry tracking, stock alerts
- ✅ **Billing**: Invoices with flexible items, discounts, tax
- ✅ **Payments**: Multiple methods, partial payments
- ✅ **Locations**: Bilingual (Arabic/English), cached
- ✅ **Error Handling**: RFC 7807 with i18n error codes
- ✅ **API Docs**: Scalar API documentation

## Technology Stack

- **.NET 10.0** - Latest framework
- **EF Core 10.0** - ORM with SQL Server
- **ASP.NET Core Identity** - User management
- **JWT + Refresh Tokens** - Authentication
- **Scalar.AspNetCore** - API documentation
- **Swashbuckle.AspNetCore** - OpenAPI generation
- **Serilog** - Structured logging
- **libphonenumber** - Phone validation
- **MailKit** - Email (SMTP)
- **GeoNames API** - Location data

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQL Server 2019+ or SQL Server Express

### Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClinicManagement;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long",
    "Issuer": "ClinicManagementAPI",
    "Audience": "ClinicManagementClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "GeoNames": {
    "Username": "your-geonames-username"
  }
}
```

### Run

```bash
dotnet restore
dotnet build
dotnet run --project src/ClinicManagement.API
```

- API: `http://localhost:5000`
- Scalar API Docs: `http://localhost:5000/scalar/v1`

Database migrations run automatically on startup.

## API Documentation

Scalar API documentation available at `/scalar/v1`

### Error Handling (RFC 7807)

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

Error codes support i18n (Arabic/English).

### Key Endpoints

#### Authentication

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get tokens
- `POST /api/auth/logout` - Logout and revoke tokens
- `GET /api/auth/me` - Get current user info
- `GET /api/auth/check-email` - Check email availability
- `GET /api/auth/check-username` - Check username availability
- `POST /api/auth/change-password` - Change password
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token

#### Patients

- `GET /api/patients` - List patients (paginated, searchable)
- `POST /api/patients` - Create patient
- `GET /api/patients/{id}` - Get patient details
- `PUT /api/patients/{id}` - Update patient
- `DELETE /api/patients/{id}` - Soft delete patient

#### Appointments

- `GET /api/appointments` - List appointments
- `POST /api/appointments` - Create appointment
- `GET /api/appointments/{id}` - Get appointment details
- `POST /api/appointments/{id}/confirm` - Confirm appointment
- `POST /api/appointments/{id}/complete` - Complete appointment
- `POST /api/appointments/{id}/cancel` - Cancel appointment

#### Invoices & Payments

- `GET /api/invoices/patient/{patientId}` - List patient invoices
- `POST /api/invoices` - Create invoice
- `GET /api/invoices/{id}` - Get invoice details
- `POST /api/invoices/{id}/cancel` - Cancel invoice
- `POST /api/invoices/{id}/payments` - Record payment
- `GET /api/invoices/{id}/payments` - Get invoice payments

#### Medicines

- `GET /api/medicines` - List medicines
- `POST /api/medicines` - Create medicine
- `GET /api/medicines/{id}` - Get medicine details
- `POST /api/medicines/{id}/stock/add` - Add stock
- `POST /api/medicines/{id}/stock/remove` - Remove stock

#### Locations

- `GET /api/locations/countries` - Get countries (cached 24h)
- `GET /api/locations/states?countryId={id}` - Get states
- `GET /api/locations/cities?stateId={id}` - Get cities

## Project Statistics

- **15 Feature Areas** - Organized by business capability
- **53 Endpoints** - RESTful API endpoints
- **44 Domain Entities** - Rich domain models
- **36 Entity Configurations** - EF Core mappings
- **18 Infrastructure Services** - Cross-cutting concerns
- **15 Domain Enums** - Type-safe enumerations
- **2 Middleware** - Global exception handling, JWT cookie handling

## License

MIT
