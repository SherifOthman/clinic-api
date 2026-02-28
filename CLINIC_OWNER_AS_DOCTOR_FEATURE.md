# Clinic Owner as Doctor Feature

## Overview

This feature allows clinic owners to be doctors and provides an option for them to set themselves as doctors after onboarding if they didn't do it during the initial setup.

## Changes Made

### 1. Seeded Clinic Owner Now Has Doctor Role

**File:** `src/ClinicManagement.Infrastructure/Persistence/Seeders/SuperAdminSeedService.cs`

- Added Doctor role to the seeded clinic owner (owner@clinic.com)
- The seeded owner now has both ClinicOwner and Doctor roles

**File:** `src/ClinicManagement.Infrastructure/Persistence/Seeders/ClinicOwnerSeedService.cs`

- Updated the seeded clinic owner to have a doctor profile with 5 years of experience
- Automatically creates Staff and DoctorProfile records for the seeded owner
- Links the doctor profile to "General Practice" specialization

### 2. New Endpoint: Set Owner as Doctor

**Endpoint:** `POST /api/staff/set-owner-as-doctor`

**Purpose:** Allows clinic owners to register themselves as doctors after completing onboarding

**Request Body:**

```json
{
  "specializationId": "guid (optional)",
  "licenseNumber": "string (optional, max 50 chars)",
  "yearsOfExperience": "integer (optional, >= 0)"
}
```

**Response:**

- `204 No Content` - Success
- `400 Bad Request` - Validation error or already registered as doctor
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not a clinic owner

**Files Created:**

- `src/ClinicManagement.Application/Staff/Commands/SetOwnerAsDoctor/SetOwnerAsDoctor.cs`
- `src/ClinicManagement.Application/Staff/Commands/SetOwnerAsDoctor/SetOwnerAsDoctorValidator.cs`
- `src/ClinicManagement.API/Contracts/Staff/StaffRequests.cs` (updated)

**Files Updated:**

- `src/ClinicManagement.API/Controllers/StaffController.cs` - Added new endpoint
- `src/ClinicManagement.Domain/Common/Constants/ErrorCodes.cs` - Added ALREADY_EXISTS and CLINIC_NOT_FOUND error codes

### 3. Business Logic

#### SetOwnerAsDoctor Command Handler

The handler performs the following steps:

1. **Validation:**
   - Verifies the user exists
   - Checks if user has ClinicOwner role
   - Ensures the user has completed onboarding (has a clinic)
   - Prevents duplicate doctor profiles

2. **Role Assignment:**
   - Adds Doctor role to the user if not already present

3. **Staff Record Creation:**
   - Creates a Staff record if it doesn't exist
   - Sets the staff as active with current hire date
   - Marks it as the primary clinic

4. **Doctor Profile Creation:**
   - Creates a DoctorProfile linked to the staff record
   - Includes optional specialization, license number, and years of experience

### 4. Error Codes Added

- `ALREADY_EXISTS` - User is already registered as a doctor
- `CLINIC_NOT_FOUND` - Clinic not found (onboarding not completed)

## Usage Scenarios

### Scenario 1: During Onboarding

When completing onboarding, clinic owners can set `ProvideMedicalServices: true` to immediately register as a doctor.

### Scenario 2: After Onboarding

If a clinic owner didn't register as a doctor during onboarding, they can use the new endpoint:

```bash
POST /api/staff/set-owner-as-doctor
Authorization: Bearer {token}
Content-Type: application/json

{
  "specializationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "licenseNumber": "DOC-12345",
  "yearsOfExperience": 10
}
```

## Testing

### Seeded Data

The seeded clinic owner (owner@clinic.com) now:

- Has both ClinicOwner and Doctor roles
- Has a Staff record in the Demo Medical Clinic
- Has a DoctorProfile with General Practice specialization
- Has 5 years of experience

### Manual Testing

1. Login as a clinic owner who hasn't registered as a doctor
2. Call the `/api/staff/set-owner-as-doctor` endpoint
3. Verify the user now has Doctor role
4. Verify Staff and DoctorProfile records are created
5. Try calling the endpoint again - should return error "You are already registered as a doctor"

## Database Impact

No migration required - uses existing tables:

- `AspNetUserRoles` - For Doctor role assignment
- `Staff` - For staff record
- `DoctorProfiles` - For doctor profile information

## Security

- Endpoint requires authentication
- Only users with ClinicOwner role can use this endpoint
- Validates that the user has completed onboarding
- Prevents duplicate doctor profiles
