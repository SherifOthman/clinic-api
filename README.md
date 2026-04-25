# Clinic Management API

A production-ready multi-tenant SaaS backend for medical clinics, built with .NET 10 and Clean Architecture. This is not a tutorial project — it models a real business domain with proper separation of concerns, a full authentication system, permission-based authorization, background jobs, audit logging, and a bilingual data layer.

**Live API Docs**: http://clinic-api.runasp.net/scalar/v1  
**Dashboard**: https://clinic-dashboard-ecru.vercel.app  
**Website**: https://clinic-website-lime.vercel.app

**Repositories**: [API](https://github.com/SherifOthman/clinic-api) • [Dashboard](https://github.com/SherifOthman/clinic-dashboard) • [Website](https://github.com/SherifOthman/clinic-website)

---

## The Problem It Solves

Small and mid-sized medical clinics need a way to manage their patients, staff, and operations — but off-the-shelf solutions are either too expensive, too generic, or don't support Arabic. This platform lets a clinic owner sign up, set up their clinic in minutes, invite their team, and start managing patients immediately. Multiple clinics run on the same platform with complete data isolation between them.

---

## What's Built

### Authentication & Identity

A complete auth system built on top of ASP.NET Identity. Users can register, confirm their email, log in with either email or username, reset their password, and manage their profile including a profile image. Token refresh is handled automatically. The system supports two client types via the `X-Client-Type` header: web clients get HTTP-only refresh token cookies (`SameSite=None; Secure` for cross-site support), mobile clients get tokens in the response body.

### Permission-Based Authorization

Fine-grained access control using a custom RBAC system. Permissions (e.g. `ViewPatients`, `InviteStaff`, `ManageBranches`) are assigned per `ClinicMember` — the same user can have different permissions at different clinics. Role defaults are defined in `DefaultPermissions.cs` and seeded when a new staff member joins; clinic owners can then customize permissions per staff member.

Permissions are resolved from the database on login and cached in `IMemoryCache` per member (10-minute TTL with explicit invalidation on change). The JWT contains only the `MemberId` claim — permissions are resolved from cache on each request by a custom `PermissionAuthorizationHandler`, keeping tokens small and avoiding stale permission data. A dynamic `IAuthorizationPolicyProvider` generates policies on-demand from the `Permission` enum, so adding a new permission requires no DI changes.

Controllers use `[RequirePermission(Permission.X)]` instead of `[Authorize(Roles = "...")]`. Structural operations (managing permissions, locking schedules) use `[Authorize(Policy = "RequireClinicOwner")]`. Platform-level operations use `[Authorize(Policy = "SuperAdmin")]`.

### Multi-Tenant Clinic Management

Every entity that belongs to a clinic implements `ITenantEntity` with a `ClinicId`. EF Core global query filters enforce tenant isolation automatically — a query from Clinic A can never return data from Clinic B. The `ICurrentUserService` extracts the `ClinicId` from JWT claims and injects it into every scoped operation. The `SuperAdmin` role bypasses these filters to see across all clinics.

### Location Data

Countries, states/governorates, and cities are seeded from GeoNames bulk dump files. Countries and states seed synchronously at startup (fast). Cities (~3.8M rows) seed in the background via a Hangfire job that runs every 2 minutes, inserts missing rows in batches, and removes itself when complete. All location data is stored in both English and Arabic. Patient and branch records store GeoNames integer IDs as foreign keys; names are resolved server-side on every query — no external API calls at runtime.

### Onboarding Flow

New clinic owners go through a guided setup: clinic name, branch details, location (country/state/city from the seeded GeoNames database), subscription plan selection, and medical specialization. The clinic is marked as active only after onboarding completes.

### Patient Management

Patients have a per-clinic sequential code (e.g. `0001`, `0042`) generated atomically via a `PatientCounters` table using a `MERGE` statement — no race conditions under concurrent inserts. The code is stored as a zero-padded string for `StartsWith` search support. Full demographics, multiple phone numbers (validated with libphonenumber-csharp), blood type, date of birth, chronic diseases, and a bilingual location. Soft-delete is supported — deleted patients are retained and can be restored by a SuperAdmin.

### Staff & Invitations

Clinic owners invite staff by email with a role (Doctor or Receptionist). The invitation has a 7-day expiry, can be resent or canceled, and contains a secure token. When the invitee clicks the link, they register and are automatically linked to the clinic with default permissions for their role. Doctors get a `DoctorInfo` with specialization and a per-branch schedule. The clinic owner can also register themselves as a doctor.

### Audit Trail

Security events (login, logout, failed attempts, account lockouts) and business events (password changes, staff invitations, permission changes, clinic onboarding) are logged via a MediatR `AuditBehavior` pipeline — any command implementing `IAuditableCommand` is automatically audited on success. Login/logout events are logged manually since they audit failures too. The SuperAdmin can query the full audit trail across all clinics filtered by entity type, action, user, clinic, or date range. Standard logs are purged after 1 month, security events after 3 months, via a batched Hangfire job.

### Subscription Plans

Plans define limits (max branches, max staff, max patients per month, storage) and feature flags (inventory management, reporting, API access, custom branding, priority support). The domain model includes billing logic like yearly discount calculation and limit checks.

### Background Jobs (Hangfire)

| Job                                 | Schedule                   | Purpose                                      |
| ----------------------------------- | -------------------------- | -------------------------------------------- |
| `CitySeedJob`                       | Every 2 min (self-removes) | Seeds ~3.8M cities in background             |
| `EmailQueueProcessorJob`            | Every 5 min                | Processes up to 50 pending emails with retry |
| `AuditLogCleanupService`            | Daily midnight             | Deletes old audit logs in batches of 5,000   |
| `RefreshTokenCleanupService`        | Every 6 hours              | Removes expired/revoked tokens               |
| `UsageMetricsAggregationJob`        | Daily 1am                  | Aggregates clinic usage metrics              |
| `SubscriptionExpiryNotificationJob` | Daily 9am                  | Sends expiry warnings 7 days before end      |

---

## Architecture

Clean Architecture with four layers. Dependencies point strictly inward — the Domain layer has zero external dependencies.

```
API            → Controllers, middleware, OpenAPI/Scalar docs
Application    → CQRS handlers, FluentValidation, MediatR pipeline behaviors
Domain         → Entities, enums, value objects, Result<T>, domain logic
Infrastructure → EF Core, ASP.NET Identity, email, file storage, background jobs
```

**CQRS with MediatR** — every operation is either a Command (write) or a Query (read). Four pipeline behaviors run on every request: `LoggingBehavior`, `ValidationBehavior` (FluentValidation, returns structured errors before the handler executes), `PerformanceBehavior` (warns on slow handlers), and `AuditBehavior` (writes an `AuditLog` entry after any command implementing `IAuditableCommand` succeeds).

**Result pattern** — handlers return `Result<T>` instead of throwing exceptions for expected failures. `GlobalExceptionMiddleware` catches anything unexpected and returns RFC 7807 Problem Details with a trace ID. Error codes are string constants that map directly to frontend i18n keys.

**No generic repository** — handlers access data through `IUnitOfWork`, which exposes typed repositories. No direct DbContext access outside the Persistence layer.

**Production schema** — all PKs use `Guid.CreateVersion7()` (time-ordered, eliminates SQL Server index fragmentation). Financial entities (`Invoice`, `Appointment`) inherit `AuditableTenantEntity` for full audit trail and automatic tenant filtering. `FinalPrice` on appointments is always set via `ApplyPrice()` to prevent stale values.

---

## Tech Stack

| Layer            | Technology                           |
| ---------------- | ------------------------------------ |
| Runtime          | .NET 10                              |
| ORM              | Entity Framework Core                |
| Identity         | ASP.NET Core Identity                |
| Mediator         | MediatR                              |
| Validation       | FluentValidation                     |
| Mapping          | Mapster                              |
| Auth             | JWT Bearer + HTTP-only cookies       |
| Authorization    | Custom RBAC (permissions + policies) |
| Background jobs  | Hangfire                             |
| Logging          | Serilog (console + rolling file)     |
| API Docs         | Scalar (OpenAPI)                     |
| Email            | MailKit + SMTP queue                 |
| Phone validation | libphonenumber-csharp                |
| Location data    | GeoNames bulk dumps (seeded DB)      |
| Database         | SQL Server                           |

---

## Feature Status

> ✅ Done · 🔧 API done, no UI · 🗂️ Domain modeled, no API or UI · ❌ Not started

### Authentication & User Management

| Feature                                | API | Notes                        |
| -------------------------------------- | --- | ---------------------------- |
| Register, email confirmation, resend   | ✅  | Token-based                  |
| Login (email or username)              | ✅  |                              |
| Logout                                 | ✅  | Clears cookie + token        |
| Forgot / reset / change password       | ✅  |                              |
| JWT + refresh token (auto-rotate)      | ✅  |                              |
| HTTP-only cookie mode (web)            | ✅  | SameSite=None for cross-site |
| Response body token mode (mobile)      | ✅  | Via `X-Client-Type: mobile`  |
| Profile — name, username, phone, image | ✅  |                              |
| In-app notifications                   | 🗂️  | Entity modeled, no API       |

### Permissions & Authorization

| Feature                              | API | Notes                                     |
| ------------------------------------ | --- | ----------------------------------------- |
| Permission enum (15 permissions)     | ✅  | Patients, staff, branches, schedule, etc. |
| Per-member permission assignment     | ✅  | Clinic owner sets per staff member        |
| Role default permissions (code-only) | ✅  | `DefaultPermissions.cs`, no DB table      |
| Dynamic policy provider              | ✅  | No foreach loop — on-demand from enum     |
| Permission cache (IMemoryCache)      | ✅  | 10-min TTL, invalidated on change         |
| Permission change audit trail        | ✅  | Logs granted/revoked per change           |
| MemberId in JWT (not permissions)    | ✅  | Keeps tokens small, no stale data         |

### Clinic & Branch Management

| Feature                                          | API | Notes                                        |
| ------------------------------------------------ | --- | -------------------------------------------- |
| Onboarding wizard (name, branch, location, plan) | ✅  |                                              |
| View / create / edit / toggle branches           | ✅  | Bilingual location                           |
| Branch phone numbers                             | ✅  |                                              |
| Clinic subscription management                   | 🗂️  | `ClinicSubscription` modeled                 |
| Subscription payment history                     | 🗂️  | `SubscriptionPayment` modeled                |
| Usage metrics / limits tracking                  | 🔧  | Background job aggregates daily, no endpoint |

### Patient Management

| Feature                                                  | API | Notes                             |
| -------------------------------------------------------- | --- | --------------------------------- |
| Paginated list — search, sort, filter by gender / region | ✅  | Search ranked by relevance        |
| Create / edit / view / soft-delete / restore             | ✅  | Restore is SuperAdmin only        |
| Per-clinic sequential patient code                       | ✅  | Atomic MERGE, StartsWith search   |
| Multiple phone numbers                                   | ✅  | International format validation   |
| Blood type, DOB, chronic diseases                        | ✅  |                                   |
| Bilingual location (country / state / city)              | ✅  | Seeded from GeoNames, EN+AR in DB |
| Patient location filter (by country/state/city)          | ✅  | Queries actual patient data       |
| Medical visit history                                    | 🗂️  | `MedicalVisit` entity modeled     |
| Medical files / documents                                | 🗂️  | `MedicalFile` entity modeled      |

### Staff Management

| Feature                                 | API | Notes                      |
| --------------------------------------- | --- | -------------------------- |
| View staff list                         | ✅  | Role and status filters    |
| Invite by email (Doctor / Receptionist) | ✅  | 7-day expiry token         |
| Resend / cancel invitation              | ✅  |                            |
| Accept invitation (register + join)     | ✅  | Default permissions seeded |
| Activate / deactivate staff             | ✅  |                            |
| Register owner as doctor                | ✅  |                            |
| Doctor specialization                   | ✅  | Set during invitation      |
| Doctor working schedule                 | ✅  |                            |
| Set / get staff permissions             | ✅  | Owner only                 |
| Schedule lock (prevent self-management) | ✅  | Owner only                 |

### Appointments

| Feature                                                  | API | Notes                                  |
| -------------------------------------------------------- | --- | -------------------------------------- |
| Book appointment                                         | 🗂️  | Entity with status, queue number, type |
| Appointment types (bilingual)                            | 🗂️  |                                        |
| Status flow (Pending → InProgress → Completed/Cancelled) | 🗂️  |                                        |
| Queue management                                         | 🗂️  | `QueueNumber` on appointment           |
| Auto-create invoice on payment                           | 🗂️  | Flow designed, not implemented         |
| Calendar view                                            | ❌  |                                        |

### Medical Visits

| Feature                                    | API | Notes                                                              |
| ------------------------------------------ | --- | ------------------------------------------------------------------ |
| Create visit linked to appointment         | 🗂️  | With diagnosis field                                               |
| Prescriptions                              | 🗂️  | Dosage, frequency, duration, instructions                          |
| Lab test orders                            | 🗂️  | Full lifecycle: Ordered → InProgress → ResultsAvailable → Reviewed |
| Radiology orders                           | 🗂️  | Same lifecycle, image + report file paths                          |
| Vital measurements                         | 🗂️  | JSON column per measurement attribute                              |
| Upload medical files                       | 🗂️  | File type enum                                                     |
| Lab / radiology test catalogs (per clinic) | 🗂️  |                                                                    |

### Inventory

| Feature                         | API | Notes                                      |
| ------------------------------- | --- | ------------------------------------------ |
| Medicine inventory (per branch) | 🗂️  | Boxes/strips, expiry, low-stock thresholds |
| Medicine dispensing             | 🗂️  | Dispensed / Partial / Cancelled status     |
| Medical supplies                | 🗂️  | Quantity + unit price                      |
| Medical services catalog        | 🗂️  | Per-branch, surgical flag                  |
| Low stock + expiry alerts       | 🗂️  | Domain logic exists, no API                |

### Billing

| Feature                                         | API | Notes                                               |
| ----------------------------------------------- | --- | --------------------------------------------------- |
| Create invoice (standalone or from appointment) | 🗂️  | AppointmentId and MedicalVisitId are nullable       |
| Line items                                      | 🗂️  | Services, medicines, supplies, lab tests, radiology |
| Exactly-one-source DB constraint                | ✅  | `CK_InvoiceItem_ExactlyOneSource`                   |
| Discounts and tax                               | 🗂️  |                                                     |
| Status flow (Draft → Issued → Paid / Cancelled) | 🗂️  | `IsOverdue` domain method                           |
| Payments (Cash, Card, etc.)                     | 🗂️  | Reference number supported                          |

### Dashboard & Analytics

| Feature                                                   | API | Notes                         |
| --------------------------------------------------------- | --- | ----------------------------- |
| Clinic stats (patients, staff, invitations, subscription) | ✅  |                               |
| Recent patients widget                                    | ✅  | Last 5                        |
| SuperAdmin cross-clinic stats                             | ✅  |                               |
| Usage metrics                                             | 🔧  | Aggregated daily, no endpoint |
| Appointment / revenue reports                             | ❌  |                               |

### Audit & Compliance

| Feature                                                         | API | Notes                                                    |
| --------------------------------------------------------------- | --- | -------------------------------------------------------- |
| Security & business event logging (MediatR AuditBehavior)       | ✅  | Login, password, staff, permissions, onboarding          |
| Permission change logging                                       | ✅  | Granted/revoked diff per change                          |
| Audit log query (filter by entity, action, user, clinic, date)  | ✅  | SuperAdmin only                                          |
| Batched retention cleanup (1 month standard, 3 months security) | ✅  | Hangfire job, 5,000 rows/batch                           |

### Reference Data

| Feature                      | API | Notes                              |
| ---------------------------- | --- | ---------------------------------- |
| Chronic diseases list        | ✅  | Bilingual, seeded                  |
| Medical specializations list | ✅  | Bilingual, seeded, anonymous       |
| Subscription plans list      | ✅  | Anonymous, includes feature flags  |
| Countries / states / cities  | ✅  | From seeded GeoNames DB, anonymous |

---

## Getting Started

```bash
# Apply migrations
dotnet ef database update --project src/ClinicManagement.Persistence --startup-project src/ClinicManagement.API

# Run
dotnet run --project src/ClinicManagement.API
```

Demo credentials are seeded automatically in Development. See `appsettings.Development.json` under the `Seed` section.

---

## License

MIT
