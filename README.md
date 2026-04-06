# Clinic Management API

Multi-tenant SaaS backend for clinic operations built with .NET 10 and Clean Architecture.

**Live Demo**: http://clinic-api.runasp.net/scalar/v1  
**Dashboard**: https://clinic-dashboard-ecru.vercel.app  
**Website**: https://clinic-website-lime.vercel.app

**Repositories**: [API](https://github.com/SherifOthman/clinic-api) • [Dashboard](https://github.com/SherifOthman/clinic-dashboard) • [Website](https://github.com/SherifOthman/clinic-website)

---

## Tech Stack

.NET 10, EF Core, ASP.NET Identity, MediatR, Mapster, FluentValidation, JWT, Serilog, Scalar, MailKit, libphonenumber-csharp, GeoNames API

---

## Architecture

Clean Architecture — 4 layers, dependencies point inward:

```
API          → Controllers, GlobalExceptionMiddleware, Scalar docs
Application  → CQRS handlers, FluentValidation, MediatR pipeline behaviors
Domain       → Entities, enums, Result<T>, zero external dependencies
Infrastructure → EF Core, ASP.NET Identity, services, background jobs, seeders
```

**CQRS with MediatR** — commands for writes, queries for reads. Three pipeline behaviors: `LoggingBehavior`, `ValidationBehavior`, `PerformanceBehavior`.

**No repository pattern** — handlers access `IApplicationDbContext` directly. Keeps things simple without the abstraction overhead.

---

## Key Features

### Authentication

- Hybrid token strategy: web clients get refresh tokens in HTTP-only cookies, mobile gets them in the response body
- Access tokens (60 min) + refresh tokens (30 days) with rotation
- Email confirmation and password reset workflows
- Role-based authorization: `ClinicOwner`, `Doctor`, `Receptionist`, `SuperAdmin`

### Multi-Tenancy

- Every tenant-scoped entity implements `ITenantEntity` with a `ClinicId`
- Global EF Core query filters applied automatically via reflection in `OnModelCreating`
- `ICurrentUserService` extracts `ClinicId` from JWT claims

### Audit Logging

- `AuditEntryBuilder` captures every Create/Update/Delete across all `AuditableEntity` types
- Soft-delete and restore detected via `IsDeleted` flag changes
- Field-level diffs serialized as `{ "Field": { "Old": "...", "New": "..." } }`
- Security events (login, logout, failed attempts, lockouts) written via `ISecurityAuditWriter`

### Error Handling

- `Result<T>` pattern — no exceptions for flow control
- `GlobalExceptionMiddleware` returns RFC 7807 Problem Details
- Standardized `ErrorCodes` constants map directly to frontend i18n keys

### Background Jobs

- `RefreshTokenCleanupService` — daily cleanup of expired/revoked tokens
- `AuditLogCleanupService` — 12-month retention policy
- `EmailQueueProcessorJob` — async email delivery
- `SubscriptionExpiryNotificationJob` — expiry alerts
- `UsageMetricsAggregationJob` — clinic usage tracking

### Seeding

- Demo users (SuperAdmin, ClinicOwner, Doctor, Receptionist) with credentials configurable via `SeedOptions` in `appsettings.json` — no hardcoded passwords in code

---

## Project Structure

```
src/
├── ClinicManagement.Domain/
│   ├── Entities/            # 43 entities across 9 modules
│   ├── Enums/               # BloodType, AuditAction, etc. + BloodTypeExtensions
│   └── Common/              # Result<T>, BaseEntity, AuditableEntity, ITenantEntity
├── ClinicManagement.Application/
│   ├── Features/
│   │   ├── Auth/            # 12 commands, 3 queries
│   │   ├── Audit/           # GetAuditLogs query
│   │   ├── Patients/        # 4 commands, 2 queries
│   │   ├── Staff/           # 6 commands, 5 queries
│   │   └── Onboarding/      # CompleteOnboarding command
│   ├── Abstractions/        # IApplicationDbContext, ICurrentUserService, ISecurityAuditWriter, etc.
│   └── Behaviors/           # Logging, Validation, Performance
├── ClinicManagement.Infrastructure/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   ├── AuditEntryBuilder.cs
│   │   ├── Configurations/
│   │   └── Seeders/         # SeedOptions + 5 seed services
│   └── Services/            # 16 services + 5 background jobs
└── ClinicManagement.API/
    ├── Controllers/         # 9 controllers, BaseApiController with HandleResult/HandleNoContent
    └── Middleware/          # GlobalExceptionMiddleware
```

---

## Getting Started

```bash
# Apply migrations
dotnet ef database update --project src/ClinicManagement.Infrastructure --startup-project src/ClinicManagement.API

# Run
dotnet run --project src/ClinicManagement.API
```

Demo credentials are in `appsettings.Development.json` under the `Seed` section.

---

## License

MIT
