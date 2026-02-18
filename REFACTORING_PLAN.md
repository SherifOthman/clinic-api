# Architecture Refactoring Implementation Plan

**Status**: Ready to Execute  
**Estimated Time**: 2-3 hours  
**Risk Level**: Medium (requires careful namespace updates)

---

## Overview

This document provides a step-by-step plan to refactor the application structure according to Clean Architecture best practices identified in `ARCHITECTURE_ANALYSIS.md`.

---

## Phase 1: Application Layer Refactoring

### Step 1.1: Create New Folder Structure

```bash
# Create Abstractions folders
mkdir -p clinic-api/src/ClinicManagement.Application/Abstractions/Authentication
mkdir -p clinic-api/src/ClinicManagement.Application/Abstractions/Email
mkdir -p clinic-api/src/ClinicManagement.Application/Abstractions/Storage
mkdir -p clinic-api/src/ClinicManagement.Application/Abstractions/Services

# Create root-level folders
mkdir -p clinic-api/src/ClinicManagement.Application/Behaviors
mkdir -p clinic-api/src/ClinicManagement.Application/Extensions
mkdir -p clinic-api/src/ClinicManagement.Application/Validation
```

### Step 1.2: Move and Update Interfaces

**Authentication Interfaces** → `Abstractions/Authentication/`

- IPasswordHasher.cs
- ITokenService.cs
- ITokenGenerator.cs

**Email Interfaces** → `Abstractions/Email/`

- IEmailService.cs
- IEmailConfirmationService.cs

**Storage Interfaces** → `Abstractions/Storage/`

- IFileStorageService.cs

**Service Interfaces** → `Abstractions/Services/`

- ICurrentUserService.cs
- IRefreshTokenService.cs

**Namespace Changes**:

- FROM: `ClinicManagement.Application.Common.Interfaces`
- TO: `ClinicManagement.Application.Abstractions.{Category}`

### Step 1.3: Move Behaviors, Extensions, Validation

**Move Files**:

- `Common/Behaviors/*` → `Behaviors/`
- `Common/Extensions/*` → `Extensions/`
- `Common/Validation/*` → `Validation/`

**Namespace Changes**:

- FROM: `ClinicManagement.Application.Common.Behaviors`
- TO: `ClinicManagement.Application.Behaviors`
- FROM: `ClinicManagement.Application.Common.Extensions`
- TO: `ClinicManagement.Application.Extensions`
- FROM: `ClinicManagement.Application.Common.Validation`
- TO: `ClinicManagement.Application.Validation`

### Step 1.4: Flatten Features Folder

**Move**:

- `Features/Auth/` → `Auth/`
- `Features/SubscriptionPlans/` → `SubscriptionPlans/`

**Namespace Changes**:

- FROM: `ClinicManagement.Application.Features.Auth`
- TO: `ClinicManagement.Application.Auth`
- FROM: `ClinicManagement.Application.Features.SubscriptionPlans`
- TO: `ClinicManagement.Application.SubscriptionPlans`

### Step 1.5: Delete Old Folders

```bash
# After moving all files
rm -rf clinic-api/src/ClinicManagement.Application/Common/Interfaces
rm -rf clinic-api/src/ClinicManagement.Application/Common/Behaviors
rm -rf clinic-api/src/ClinicManagement.Application/Common/Extensions
rm -rf clinic-api/src/ClinicManagement.Application/Common/Validation
rm -rf clinic-api/src/ClinicManagement.Application/Features
```

### Step 1.6: Update All Import Statements

**Files to Update** (use Find & Replace in IDE):

1. All files in `Application/Auth/`
2. All files in `Application/SubscriptionPlans/`
3. All files in `Infrastructure/Services/`
4. `Infrastructure/DependencyInjection.cs`
5. `API/Controllers/AuthController.cs`
6. `API/Controllers/SubscriptionPlansController.cs`

**Find & Replace Operations**:

```
Find: using ClinicManagement.Application.Common.Interfaces;
Replace with specific imports based on what's used:
  using ClinicManagement.Application.Abstractions.Authentication;
  using ClinicManagement.Application.Abstractions.Email;
  using ClinicManagement.Application.Abstractions.Services;
  using ClinicManagement.Application.Abstractions.Storage;

Find: using ClinicManagement.Application.Features.Auth
Replace: using ClinicManagement.Application.Auth

Find: using ClinicManagement.Application.Features.SubscriptionPlans
Replace: using ClinicManagement.Application.SubscriptionPlans

Find: using ClinicManagement.Application.Common.Behaviors
Replace: using ClinicManagement.Application.Behaviors

Find: using ClinicManagement.Application.Common.Extensions
Replace: using ClinicManagement.Application.Extensions

Find: using ClinicManagement.Application.Common.Validation
Replace: using ClinicManagement.Application.Validation
```

### Step 1.7: Handle Common/Models and Common/Options

**Options** - Move to Infrastructure or delete:

- `JwtOptions.cs` → Infrastructure (or inline in DependencyInjection)
- `SmtpOptions.cs` → Infrastructure (or inline in DependencyInjection)
- `CookieSettings.cs` → Infrastructure
- `CorsOptions.cs` → API layer
- `GeoNamesOptions.cs` → Infrastructure
- `FileStorageOptions.cs` → Keep in Application (business rules)

**Models** - Redistribute:

- `MessageResponse.cs` → API/Models/
- `ProblemDetails.cs` → API/Models/ (rename to ApiProblemDetails if not already)
- `AccessTokenValidationResult.cs` → Check if used, move to Infrastructure or delete
- `UserRegistrationRequest.cs` → Application/Auth/Contracts/

---

## Phase 2: Infrastructure Layer Refactoring

### Step 2.1: Create New Folder Structure

```bash
mkdir -p clinic-api/src/ClinicManagement.Infrastructure/Authentication
mkdir -p clinic-api/src/ClinicManagement.Infrastructure/Email
mkdir -p clinic-api/src/ClinicManagement.Infrastructure/Persistence/Repositories
mkdir -p clinic-api/src/ClinicManagement.Infrastructure/Persistence/Scripts
mkdir -p clinic-api/src/ClinicManagement.Infrastructure/Storage
mkdir -p clinic-api/src/ClinicManagement.Infrastructure/BackgroundJobs
mkdir -p clinic-api/src/ClinicManagement.Infrastructure/Identity
```

### Step 2.2: Reorganize Services by Concern

**Authentication** → `Authentication/`

- PasswordHasher.cs
- TokenService.cs
- TokenGenerator.cs
- CookieService.cs

**Email** → `Email/`

- EmailService.cs
- SmtpEmailSender.cs
- MailKitSmtpClient.cs
- EmailTemplates.cs

**Persistence** → `Persistence/`

- Rename `Data/` folder to `Persistence/`
- Keep Repositories, Scripts, UnitOfWork, DbUpMigrationService

**Storage** → `Storage/`

- LocalFileStorageService.cs

**BackgroundJobs** → `BackgroundJobs/`

- RefreshTokenCleanupService.cs

**Identity** → `Identity/`

- EmailConfirmationService.cs
- RefreshTokenService.cs
- SuperAdminSeedService.cs

**Services** (cross-cutting only) → `Services/`

- CurrentUserService.cs
- DateTimeProvider.cs
- PhoneValidationService.cs
- GeoNamesService.cs

### Step 2.3: Update Namespaces

**Namespace Changes**:

- FROM: `ClinicManagement.Infrastructure.Services`
- TO: `ClinicManagement.Infrastructure.{Category}` (Authentication, Email, etc.)
- FROM: `ClinicManagement.Infrastructure.Data`
- TO: `ClinicManagement.Infrastructure.Persistence`

### Step 2.4: Update DependencyInjection.cs

Update service registrations to reflect new namespaces and organization.

---

## Phase 3: API Layer Cleanup

### Step 3.1: Create Models Folder

```bash
mkdir -p clinic-api/src/ClinicManagement.API/Models
```

### Step 3.2: Move API Models

Move from Application/Common/Models:

- MessageResponse.cs → API/Models/
- ApiProblemDetails.cs → API/Models/

Update namespaces and imports.

---

## Phase 4: Verification

### Step 4.1: Build Solution

```bash
dotnet build clinic-api/ClinicManagement.sln
```

Fix any compilation errors.

### Step 4.2: Run Tests (if available)

```bash
dotnet test clinic-api/ClinicManagement.sln
```

### Step 4.3: Verify Structure

Check that folder structure matches recommended structure from ARCHITECTURE_ANALYSIS.md.

---

## Phase 5: Cleanup

### Step 5.1: Delete Empty Folders

```bash
# Delete old Common folder if empty
rm -rf clinic-api/src/ClinicManagement.Application/Common

# Delete old Data folder (now Persistence)
rm -rf clinic-api/src/ClinicManagement.Infrastructure/Data

# Delete old Services folder (now organized by concern)
rm -rf clinic-api/src/ClinicManagement.Infrastructure/Services
```

### Step 5.2: Update Documentation

Update:

- FOLDER_STRUCTURE.md
- CLEAN_ARCHITECTURE_STATUS.md
- README.md (if it references folder structure)

---

## Execution Strategy

### Option A: Manual Execution (Recommended)

1. Use IDE's refactoring tools (Rename, Move)
2. Execute phase by phase
3. Build after each phase to catch errors early
4. Commit after each successful phase

### Option B: Automated Script

Create PowerShell/Bash script to:

1. Move files
2. Update namespaces using regex
3. Build and verify

### Option C: Hybrid Approach

1. Use IDE to move files (preserves git history)
2. Use script for bulk namespace updates
3. Manual verification and fixes

---

## Risk Mitigation

1. **Create backup branch** before starting
2. **Commit frequently** after each successful step
3. **Test build** after each phase
4. **Keep old structure** until new structure is verified working

---

## Expected Benefits

After completion:

- ✅ Screaming Architecture (business entities visible immediately)
- ✅ Clear separation of concerns (Abstractions folder)
- ✅ Better cohesion (related code grouped together)
- ✅ Easier navigation (flatter structure)
- ✅ Industry-standard structure (matches best practices)
- ✅ Improved maintainability

---

## Rollback Plan

If issues arise:

```bash
git reset --hard <commit-before-refactoring>
```

Or cherry-pick successful phases:

```bash
git cherry-pick <phase1-commit>
git cherry-pick <phase2-commit>
```

---

## Next Steps

1. Review this plan
2. Create backup branch: `git checkout -b backup/before-refactoring`
3. Create working branch: `git checkout -b refactor/clean-architecture-structure`
4. Execute Phase 1
5. Commit and verify
6. Continue with remaining phases

---

## Notes

- This refactoring does NOT change any business logic
- Only folder structure and namespaces are affected
- All functionality remains the same
- Build must succeed after each phase
- Consider doing this refactoring when no active feature development is happening
