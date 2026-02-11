# Clinic Management API

A modern clinic management system built with .NET 10 and **True Vertical Slice Architecture (VSA)**.

## Architecture

This project follows **Vertical Slice Architecture** as defined by Jimmy Bogard, organizing code by features rather than technical layers. Each feature is a self-contained vertical slice with everything it needs.

### Project Structure

```
ClinicManagement.API/              # Single project - VSA style
├── Features/                      # Vertical slices - each feature is self-contained
│   ├── Appointments/              # All appointment-related code
│   ├── Auth/                      # Authentication features
│   ├── Patients/                  # Patient management
│   ├── Invoices/                  # Billing features
│   ├── Medicines/                 # Medicine inventory
│   ├── Payments/                  # Payment processing
│   └── ...                        # Other features (59 total endpoints)
├── Shared/                        # Shared code used across features
│   ├── Entities/                  # Domain entities
│   ├── Events/                    # Domain events
│   ├── Common/                    # Common interfaces, models, exceptions
│   └── Specifications/            # Query specifications
├── Data/                          # Database context, repositories, migrations
│   ├── ApplicationDbContext.cs
│   ├── Repositories/
│   ├── Configurations/
│   └── Migrations/
├── Services/                      # Infrastructure services
│   ├── AuthenticationService.cs
│   ├── EmailService.cs
│   └── ...
├── DTOs/                          # Data transfer objects
├── Options/                       # Configuration options
├── Common/                        # API-level common code
├── Middleware/                    # HTTP middleware
├── Endpoints.cs                   # Central endpoint registration
└── Program.cs
```

### Key VSA Principles

1. **Feature-Centric**: Code is organized around business features, not technical layers
2. **Self-Contained Slices**: Each feature contains everything it needs (request, handler, validation, data access)
3. **Minimal Coupling Between Slices**: Features are independent and don't depend on each other
4. **Maximum Coupling Within a Slice**: All code for a feature is tightly grouped
5. **Pragmatic Sharing**: Shared code (entities, services) is kept in common folders when it makes sense

### Why VSA?

- **Easy to Navigate**: All code for a feature is in one place
- **Fast Development**: New features only add code, don't modify shared code
- **Reduced Coupling**: Changes to one feature don't affect others
- **Flexible**: Each feature can choose the best approach for its needs
- **Testable**: Features are isolated and easy to test
- **Aligns with Business**: Structure matches how users think about features

## Features

- ✅ Authentication & Authorization (JWT + Refresh Tokens)
- ✅ Patient Management
- ✅ Appointment Scheduling
- ✅ Medicine Inventory
- ✅ Billing & Invoices
- ✅ Payment Processing
- ✅ Medical Services & Supplies
- ✅ Chronic Disease Management
- ✅ Location Management (GeoNames integration)
- ✅ Subscription Plans
- ✅ Onboarding Flow
- ✅ Measurements & Attributes

## Technology Stack

- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core 10.0
- SQL Server
- MediatR for CQRS
- FluentValidation
- Mapster for object mapping
- Serilog for logging
- JWT Authentication
- Hangfire for background jobs
- MailKit for email
- libphonenumber for phone validation

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQL Server (or SQL Server Express)

### Configuration

Update `appsettings.json` with your database connection string and other settings.

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project ClinicManagement.API
```

The API will be available at `https://localhost:5001` (or the port specified in launchSettings.json).

### Database Migrations

Migrations are automatically applied on startup. To create new migrations:

```bash
dotnet ef migrations add MigrationName --project ClinicManagement.API
```

## API Documentation

Swagger UI is available at `/swagger` when running the application.

## Architecture References

- [Vertical Slice Architecture by Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
- [Structuring Vertical Slices by Milan Jovanović](https://www.milanjovanovic.tech/blog/vertical-slice-architecture-structuring-vertical-slices)
- [Vertical Slice Architecture by Julio Casal](https://juliocasal.com/blog/vertical-slice-architecture)

## License

MIT
