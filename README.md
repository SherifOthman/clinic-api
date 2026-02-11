# Clinic Management API

A comprehensive clinic management system built with .NET 10, featuring **Vertical Slice Architecture**, **Domain-Driven Design**, and **Multi-Tenancy**.

## Architecture

### Core Patterns

1. **Vertical Slice Architecture**: Features organized as independent slices
2. **Domain-Driven Design**: Rich domain models with business logic
3. **Multi-Tenancy**: Automatic clinic-based data isolation
4. **Soft Delete**: Audit trail with soft delete pattern

### Project Structure

```
ClinicManagement.API/
├── Features/                      # 15 feature areas, 53 endpoints
│   ├── Appointments/              # 6 endpoints
│   ├── Auth/                      # 15 endpoints
│   ├── Patients/                  # 5 endpoints
│   ├── Invoices/                  # 4 endpoints
│   ├── Payments/                  # 2 endpoints
│   ├── Medicines/                 # 5 endpoints
│   ├── Locations/                 # 3 endpoints (bilingual)
│   └── ...                        # Other features
├── Entities/                      # 44 domain entities
│   ├── Appointment/
│   ├── Billing/
│   ├── Clinic/
│   ├── Identity/
│   ├── Inventory/
│   ├── Medical/
│   ├── Patient/
│   └── Reference/
├── Common/                        # Shared code
│   ├── Constants/                 # ErrorCodes, Roles
│   ├── Enums/                     # 15 domain enums
│   ├── Exceptions/                # 12 domain exceptions
│   └── Extensions/
├── Infrastructure/
│   ├── Data/                      # EF Core, 35 configurations
│   ├── Services/                  # 18 services
│   └── Middleware/                # 2 middleware
└── Program.cs
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
- ✅ **API Docs**: Swagger with JWT support

## Technology Stack

- **.NET 10.0** - Latest framework
- **EF Core 10.0** - ORM with SQL Server
- **ASP.NET Core Identity** - User management
- **JWT + Refresh Tokens** - Authentication
- **Swagger** - API documentation
- **Serilog** - Structured logging
- **Hangfire** - Background jobs
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
    "Audience": "ClinicManagementClient"
  }
}
```

### Run

```bash
dotnet restore
dotnet build
dotnet run --project ClinicManagement.API
```

- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

Database migrations run automatically on startup.

## API Documentation

Swagger UI available at `/swagger`

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

- `POST /api/auth/register` - Register
- `POST /api/auth/login` - Login
- `GET /api/patients` - List patients
- `POST /api/appointments` - Create appointment
- `POST /api/invoices` - Create invoice
- `POST /api/invoices/{id}/payments` - Record payment
- `GET /api/locations/countries` - Get countries (bilingual)

## License

MIT
