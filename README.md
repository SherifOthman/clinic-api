# Clinic Management API

A production-ready multi-tenant SaaS backend for medical clinics, built with .NET 10 and Clean Architecture. This is not a tutorial project — it models a real business domain with proper separation of concerns, a full authentication system, background jobs, audit logging, and a bilingual data layer.

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

A complete auth system built on top of ASP.NET Identity. Users can register, confirm their email, log in with either email or username, reset their password, and manage their profile including a profile image. Token refresh is handled automatically. The system supports two client types via the `X-Client-Type` header: web clients get HTTP-only refresh token cookies (XSS-safe), mobile clients get tokens in the response body.

### Multi-Tenant Clinic Management

Every entity that belongs to a clinic implements `ITenantEntity` with a `ClinicId`. EF Core global query filters enforce tenant isolation automatically — a query from Clinic A can never return data from Clinic B. The `ICurrentUserService` extracts the `ClinicId` from JWT claims and injects it into every scoped operation. The `SuperAdmin` role bypasses these filters to see across all clinics.

### Location Data

Countries, states/governorates, and cities are seeded from GeoNames bulk dump files at startup. The app looks for the files in `wwwroot/SeedData/GeoNames/` — upload them there via FTP/file manager and they will never be re-downloaded. The seeder runs synchronously at startup before the app accepts requests, so the data is always fully available. It compares existing DB rows against the source file and only inserts missing entries, so it's safe to restart mid-seed and it will continue from where it left off. All location data is stored in both English and Arabic. Patient and branch records store GeoNames integer IDs as foreign keys; names are resolved server-side on every query using the seeded tables — no external API calls at runtime.

### Onboarding Flow

New clinic owners go through a guided setup: clinic name, branch details, location (country/state/city from the seeded GeoNames database), subscription plan selection, and medical specialization. The clinic is marked as active only after onboarding completes.

### Patient Management

Patients have a globally unique 8-digit code, full demographics, multiple phone numbers (validated with libphonenumber-csharp), blood type, date of birth, chronic diseases, and a bilingual location (country/state/city stored in both English and Arabic). Soft-delete is supported — deleted patients are retained in the database and can be restored by a SuperAdmin. Search results are ranked by relevance: exact code match first, then name match, then partial matches.

### Staff & Invitations

Clinic owners invite staff by email with a role (Doctor or Receptionist). The invitation has a 7-day expiry, can be resent or canceled, and contains a secure token. When the invitee clicks the link, they register and are automatically linked to the clinic. Doctors get a `DoctorProfile` with specialization. The clinic owner can also register themselves as a doctor.

### Audit Trail

Every create, update, and delete on any `AuditableEntity` is captured with field-level diffs — old value, new value, who made the change, when, from which IP, and which browser. Security events (login, logout, failed attempts, account lockouts) are logged separately. The SuperAdmin can query the full audit trail across all clinics. Logs older than 12 months are automatically purged by a background job.

### Subscription Plans

Plans define limits (max branches, max staff, max patients per month, storage) and feature flags (inventory management, reporting, API access, custom branding, priority support). The domain model includes billing logic like yearly discount calculation and limit checks.

### Background Jobs

Six hosted services run continuously in the background:

- **GeoLocationSeedService** — runs once at startup, seeds countries/states/cities from GeoNames files; skips already-inserted rows and updates Arabic names if `ar_names.tsv` is available
- **EmailQueueProcessorJob** — processes up to 50 pending emails every 5 minutes with retry logic and priority ordering
- **AuditLogCleanupService** — runs at midnight daily, deletes audit logs older than 12 months
- **RefreshTokenCleanupService** — runs every 6 hours, removes expired and revoked tokens
- **UsageMetricsAggregationJob** — aggregates clinic usage metrics hourly for billing and analytics
- **SubscriptionExpiryNotificationJob** — sends expiry warnings to clinics approaching their subscription end date

---

## Architecture

Clean Architecture with four layers. Dependencies point strictly inward — the Domain layer has zero external dependencies.

```
API            → Controllers, middleware, OpenAPI/Scalar docs
Application    → CQRS handlers, FluentValidation, MediatR pipeline behaviors
Domain         → Entities, enums, value objects, Result<T>, domain logic
Infrastructure → EF Core, ASP.NET Identity, email, file storage, background jobs
```

**CQRS with MediatR** — every operation is either a Command (write) or a Query (read), dispatched through MediatR. Three pipeline behaviors run on every request: `LoggingBehavior` logs the handler name and duration, `ValidationBehavior` runs all FluentValidation validators and returns structured errors before the handler executes, and `PerformanceBehavior` warns when a handler takes too long.

**Result pattern** — handlers return `Result<T>` instead of throwing exceptions for expected failures. `GlobalExceptionMiddleware` catches anything unexpected and returns RFC 7807 Problem Details with a trace ID. Error codes are string constants that map directly to frontend i18n keys, so the frontend can display the right translated message without any mapping logic.

**No generic repository** — handlers access data through `IUnitOfWork`, which exposes typed repositories (`IPatientRepository`, `IGeoLocationRepository`, etc.). No direct DbContext access outside the Persistence layer.

---

## Tech Stack

| Layer            | Technology                       |
| ---------------- | -------------------------------- |
| Runtime          | .NET 10                          |
| ORM              | Entity Framework Core            |
| Identity         | ASP.NET Core Identity            |
| Mediator         | MediatR                          |
| Validation       | FluentValidation                 |
| Mapping          | Mapster                          |
| Auth             | JWT Bearer + HTTP-only cookies   |
| Logging          | Serilog (console + rolling file) |
| API Docs         | Scalar (OpenAPI)                 |
| Email            | MailKit + SMTP queue             |
| Phone validation | libphonenumber-csharp            |
| Location data    | GeoNames bulk dumps (seeded DB)  |
| Database         | SQL Server                       |

---

## Feature Status

> ✅ Done · 🔧 API done, no UI · 🗂️ Domain modeled, no API or UI · ❌ Not started

### Authentication & User Management

| Feature                                | API | Notes                       |
| -------------------------------------- | --- | --------------------------- |
| Register, email confirmation, resend   | ✅  | Token-based                 |
| Login (email or username)              | ✅  |                             |
| Logout                                 | ✅  | Clears cookie + token       |
| Forgot / reset / change password       | ✅  |                             |
| JWT + refresh token (auto-rotate)      | ✅  |                             |
| HTTP-only cookie mode (web)            | ✅  | XSS-safe                    |
| Response body token mode (mobile)      | ✅  | Via `X-Client-Type: mobile` |
| Profile — name, username, phone, image | ✅  |                             |
| In-app notifications                   | 🗂️  | Entity modeled, no API      |

### Clinic & Branch Management

| Feature                                          | API | Notes                                         |
| ------------------------------------------------ | --- | --------------------------------------------- |
| Onboarding wizard (name, branch, location, plan) | ✅  |                                               |
| View / create / edit / toggle branches           | ✅  | Bilingual location                            |
| Branch phone numbers                             | ✅  |                                               |
| Branch appointment pricing                       | 🗂️  | Entity exists                                 |
| Clinic subscription management                   | 🗂️  | `ClinicSubscription` modeled                  |
| Subscription payment history                     | 🗂️  | `SubscriptionPayment` modeled                 |
| Usage metrics / limits tracking                  | 🔧  | Background job aggregates hourly, no endpoint |

### Patient Management

| Feature                                                  | API | Notes                             |
| -------------------------------------------------------- | --- | --------------------------------- |
| Paginated list — search, sort, filter by gender / region | ✅  | Search ranked by relevance        |
| Create / edit / view / soft-delete / restore             | ✅  | Restore is SuperAdmin only        |
| Unique 8-digit patient code                              | ✅  | Auto-generated                    |
| Multiple phone numbers                                   | ✅  | International format validation   |
| Blood type, DOB, chronic diseases                        | ✅  |                                   |
| Bilingual location (country / state / city)              | ✅  | Seeded from GeoNames, EN+AR in DB |
| Patient location filter (by country/state/city)          | ✅  | Queries actual patient data       |
| Medical visit history                                    | 🗂️  | `MedicalVisit` entity modeled     |
| Medical files / documents                                | 🗂️  | `MedicalFile` entity modeled      |

### Staff Management

| Feature                                 | API | Notes                   |
| --------------------------------------- | --- | ----------------------- |
| View staff list                         | ✅  | Role and status filters |
| Invite by email (Doctor / Receptionist) | ✅  | 7-day expiry token      |
| Resend / cancel invitation              | ✅  |                         |
| Accept invitation (register + join)     | ✅  |                         |
| Activate / deactivate staff             | ✅  |                         |
| Register owner as doctor                | ✅  |                         |
| Doctor specialization                   | ✅  | Set during invitation   |
| Doctor working schedule                 | ✅  |                         |

### Appointments

| Feature                                                   | API | Notes                                  |
| --------------------------------------------------------- | --- | -------------------------------------- |
| Book appointment                                          | 🗂️  | Entity with status, queue number, type |
| Appointment types (bilingual)                             | 🗂️  |                                        |
| Status flow (Pending → Confirmed → Completed / Cancelled) | 🗂️  |                                        |
| Queue management                                          | 🗂️  | `QueueNumber` on appointment           |
| Calendar view                                             | ❌  |                                        |
| Link appointment to invoice                               | 🗂️  | FK modeled                             |

### Medical Visits

| Feature                                    | API | Notes                                                              |
| ------------------------------------------ | --- | ------------------------------------------------------------------ |
| Create visit linked to appointment         | 🗂️  | With diagnosis field                                               |
| Prescriptions                              | 🗂️  | Dosage, frequency, duration, instructions                          |
| Lab test orders                            | 🗂️  | Full lifecycle: Ordered → InProgress → ResultsAvailable → Reviewed |
| Radiology orders                           | 🗂️  | Same lifecycle, image + report file paths                          |
| Vital measurements                         | 🗂️  | EAV model — each doctor configures their own fields                |
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
| Create invoice                                  | 🗂️  | Linked to appointment or visit                      |
| Line items                                      | 🗂️  | Services, medicines, supplies, lab tests, radiology |
| Discounts and tax                               | 🗂️  |                                                     |
| Status flow (Draft → Issued → Paid / Cancelled) | 🗂️  | `IsOverdue` domain method                           |
| Payments (Cash, Card, etc.)                     | 🗂️  | Reference number supported                          |

### Dashboard & Analytics

| Feature                                                   | API | Notes                          |
| --------------------------------------------------------- | --- | ------------------------------ |
| Clinic stats (patients, staff, invitations, subscription) | ✅  |                                |
| Recent patients widget                                    | ✅  | Last 5                         |
| SuperAdmin cross-clinic stats                             | ✅  |                                |
| Usage metrics                                             | 🔧  | Aggregated hourly, no endpoint |
| Appointment / revenue reports                             | ❌  |                                |

### Audit & Compliance

| Feature                                                        | API | Notes                                    |
| -------------------------------------------------------------- | --- | ---------------------------------------- |
| Field-level change tracking                                    | ✅  | All `AuditableEntity` types              |
| Security event logging                                         | ✅  | Login, logout, failed attempts, lockouts |
| Audit log query (filter by entity, action, user, clinic, date) | ✅  | SuperAdmin only                          |
| 12-month retention with auto-cleanup                           | ✅  | Background job                           |

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
