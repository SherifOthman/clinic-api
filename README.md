# Clinic Management API

A production-ready REST API powering a multi-tenant SaaS platform for medical clinics. Built to demonstrate real-world backend engineering — not a tutorial project.

Multiple clinics run on the same platform with complete data isolation. A clinic owner signs up, sets up their clinic, and their team (doctors, receptionists) gets access through an email invitation flow. The system tracks every action taken — who changed what, when, and from where.

**Live Demo**: http://clinic-api.runasp.net/scalar/v1  
**Dashboard**: https://clinic-dashboard-ecru.vercel.app  
**Website**: https://clinic-website-lime.vercel.app

**Repositories**: [API](https://github.com/SherifOthman/clinic-api) • [Dashboard](https://github.com/SherifOthman/clinic-dashboard) • [Website](https://github.com/SherifOthman/clinic-website)

---

## What the app does

Think of it as the backend for a clinic management platform — like a lightweight Vezeeta or Doctoralia, but self-hosted and white-labeled for individual clinics.

Each clinic is fully isolated. A clinic owner registers, sets up their clinic, and invites their team. Doctors and receptionists get access through a secure email invitation. Patients are registered and their full medical profile is tracked over time. Every action in the system — who created a record, who changed a field, who logged in from where — is captured in an immutable audit trail.

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
