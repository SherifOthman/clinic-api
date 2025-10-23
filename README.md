# 🏥 SaaS Clinic Management System

A comprehensive .NET 9 Web API application built with Clean Architecture principles for managing clinic operations, appointments, patients, and medical staff.

## 🏗️ Architecture

This project follows Clean Architecture principles with the following layers:

- **Domain Layer**: Contains entities, value objects, aggregate roots, and domain logic
- **Application Layer**: Contains use cases, DTOs, interfaces, and business logic
- **Infrastructure Layer**: Contains data access, external services, and implementations
- **API Layer**: Contains controllers, middleware, and API endpoints

## 🚀 Features

- **Authentication & Authorization**: JWT-based authentication with access and refresh tokens
- **User Management**: Complete user registration and login system
- **Clinic Management**: Multi-tenant clinic system with subscription plans
- **Patient Management**: Comprehensive patient records and medical history
- **Appointment Scheduling**: Advanced appointment booking and management
- **Medical Staff**: Doctor and receptionist management with specializations
- **Dynamic Forms**: EAV (Entity-Attribute-Value) model for custom fields
- **Prescription Management**: Medicine and prescription tracking
- **Branch Management**: Multi-location clinic support

## 🛠️ Technology Stack

- **.NET 9**: Latest .NET framework
- **Entity Framework Core**: ORM for data access
- **ASP.NET Core Identity**: User management and authentication
- **JWT Bearer Authentication**: Secure token-based authentication
- **FluentValidation**: Request validation
- **AutoMapper**: Object mapping
- **MediatR**: CQRS pattern implementation
- **SQL Server**: Database
- **Swagger/OpenAPI**: API documentation

## 📋 Prerequisites

- .NET 9 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd clinic-backend
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Update Connection String

Update the connection string in `src/ClinicManagement.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ClinicManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 4. Run Database Migrations

```bash
cd src/ClinicManagement.API
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run --project src/ClinicManagement.API
```

The API will be available at:

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:7000`
- Swagger UI: `https://localhost:7000/swagger`

## 🔐 Authentication

The API uses JWT authentication with the following endpoints:

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get access token
- `POST /api/auth/refresh-token` - Refresh access token
- `POST /api/auth/logout` - Logout and revoke refresh token

### JWT Configuration

Update the JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "ClinicManagementAPI",
    "Audience": "ClinicManagementUsers",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

## 📚 API Endpoints

### Authentication

- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh-token` - Refresh token
- `POST /api/auth/logout` - User logout

### Clinics

- `GET /api/clinics` - Get all clinics
- `GET /api/clinics/{id}` - Get clinic by ID
- `POST /api/clinics` - Create new clinic
- `PUT /api/clinics/{id}` - Update clinic

### Patients

- `GET /api/patients` - Get all patients
- `GET /api/patients/{id}` - Get patient by ID
- `POST /api/patients` - Create new patient
- `PUT /api/patients/{id}` - Update patient

### Appointments

- `GET /api/appointments` - Get all appointments
- `GET /api/appointments/{id}` - Get appointment by ID
- `POST /api/appointments` - Create new appointment
- `PUT /api/appointments/{id}` - Update appointment

## 🏗️ Project Structure

```
src/
├── ClinicManagement.Domain/          # Domain layer
│   ├── Entities/                    # Domain entities
│   ├── Aggregates/                  # Aggregate roots
│   ├── Common/                      # Value objects and enums
│   └── ClinicManagement.Domain.csproj
├── ClinicManagement.Application/     # Application layer
│   ├── Common/                      # Interfaces and models
│   ├── DTOs/                        # Data transfer objects
│   ├── Features/                    # CQRS commands and queries
│   └── ClinicManagement.Application.csproj
├── ClinicManagement.Infrastructure/ # Infrastructure layer
│   ├── Data/                        # Database context
│   ├── Services/                    # External services
│   └── ClinicManagement.Infrastructure.csproj
└── ClinicManagement.API/            # API layer
    ├── Controllers/                 # API controllers
    ├── Properties/                  # Launch settings
    └── ClinicManagement.API.csproj
```

## 🔧 Development

### Adding New Features

1. **Domain**: Add entities and value objects in the Domain layer
2. **Application**: Create DTOs, commands, queries, and handlers
3. **Infrastructure**: Implement repositories and external services
4. **API**: Add controllers and endpoints

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project src/ClinicManagement.Infrastructure --startup-project src/ClinicManagement.API

# Update database
dotnet ef database update --project src/ClinicManagement.Infrastructure --startup-project src/ClinicManagement.API
```

## 🧪 Testing

The project includes comprehensive validation using FluentValidation for all input models.

## 📝 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📞 Support

For support and questions, please contact the development team.
