# Clinic Management API

A multi-tenant SaaS backend for managing medical clinics. Each clinic operates in complete isolation — patients, staff, and data are scoped to the clinic that owns them.

**Live Demo**: http://clinic-api.runasp.net/scalar/v1  
**Dashboard**: https://clinic-dashboard-ecru.vercel.app  
**Website**: https://clinic-website-lime.vercel.app

**Repositories**: [API](https://github.com/SherifOthman/clinic-api) • [Dashboard](https://github.com/SherifOthman/clinic-dashboard) • [Website](https://github.com/SherifOthman/clinic-website)

---

## What the app does

A clinic owner registers, sets up their clinic (name, branch, subscription plan, specialization), and invites doctors and receptionists via email. Once onboarded, the clinic can manage patients — registering them with their demographics, phone numbers, blood type, and chronic diseases — and track all changes through a full audit trail.

### Implemented ✅

- **Auth** — Register, login (email or username), email confirmation, password reset, profile management, profile image upload, token refresh, logout. Supports both web (HTTP-only cookie) and mobile (response body) clients.
- **Onboarding** — Clinic setup wizard: clinic name, branch details, location (country/state/city via GeoNames), subscription plan selection, specialization.
- **Patients** — Create, read, update, soft-delete, and restore patients. Each patient has phone numbers, chronic diseases, blood type, date of birth, and location. Patient codes are globally unique 8-digit identifiers.
- **Staff** — Invite staff by email (Doctor or Receptionist), resend/cancel invitations, accept invitation with registration, activate/deactivate staff members, register clinic owner as a doctor.
- **Audit Logs** — Full cross-clinic audit trail for SuperAdmin. Every create/update/delete is captured with field-level diffs, user info, IP address, and browser. Security events (login, logout, failed attempts, lockouts) are also logged.
- **Reference data** — Chronic diseases list, medical specializations, subscription plans, location lookup (countries, states, cities).

### Domain modeled but not yet implemented ⏳

The following modules have entities and database tables but no API endpoints or business logic yet:

- **Appointments** — Scheduling, confirmation, cancellation
- **Medical visits** — Visit records, prescriptions, lab tests, radiology orders, measurements
- **Billing** — Invoices, payments, discounts
- **Inventory** — Medicines, medical supplies, medical services, dispensing
- **Notifications** — In-app notification system

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

**Multi-tenancy** — every tenant-scoped entity implements `ITenantEntity` with a `ClinicId`. Global EF Core query filters applied automatically. `ICurrentUserService` extracts `ClinicId` from JWT claims.

**Audit logging** — `AuditEntryBuilder` captures field-level diffs for every `AuditableEntity`. Security events go through `ISecurityAuditWriter`. 12-month retention enforced by a background cleanup job.

**Error handling** — `Result<T>` pattern, no exceptions for flow control. `GlobalExceptionMiddleware` returns RFC 7807 Problem Details. `ErrorCodes` constants map directly to frontend i18n keys.

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
