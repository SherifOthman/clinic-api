# Multi-Tenancy and User Types Architecture

## 📋 Overview

This document describes the **single-clinic multi-tenancy** architecture and **separate user type tables** implementation for the Clinic Management System.

---

## 🏗️ Architecture Decision

### **Single-Clinic Per User Model**

**Decision**: Each user belongs to **ONE clinic only**.

**Rationale**:

- ✅ Simpler authorization logic
- ✅ Smaller JWT tokens
- ✅ Better performance (no clinic switching)
- ✅ Matches real-world clinic operations
- ✅ Clear data ownership and isolation

---

## 👥 User Types

### **1. Core Medical Staff**

#### **Doctor**

```csharp
Table: Doctors
Fields:
- UserId (FK to Users) - One-to-one
- SpecializationId (FK to Specializations)
- LicenseNumber
- YearsOfExperience
- ConsultationFee
- AvailableForEmergency
- Biography

Permissions:
- View/Create/Update Patients
- Create/Update Appointments
- Create/Update Medical Records
- Prescribe Medicines
- Order Lab Tests
- View/Create Invoices
```

#### **Nurse**

```csharp
Table: Nurses
Fields:
- UserId (FK to Users) - One-to-one
- Certification
- Department
- ShiftType (Day/Night/Rotating/Flexible)
- CanAdministerMedication

Permissions:
- View Patients
- Record Vital Signs/Measurements
- View Appointments
- Assist Doctors
- Limited Medical Record Access
```

---

### **2. Administrative Staff**

#### **Receptionist**

```csharp
Table: Receptionists
Fields:
- UserId (FK to Users) - One-to-one
- HireDate
- ShiftPreference
- CanHandlePayments
- Languages (comma-separated)

Permissions:
- Create/Update Patients (basic info)
- Create/Update/Cancel Appointments
- View Appointment Schedule
- Process Payments
- Generate Invoices
- NO access to medical records
```

#### **ClinicOwner**

```csharp
Table: ClinicOwners
Fields:
- UserId (FK to Users) - One-to-one
- BusinessLicense
- TaxId
- OwnershipPercentage
- IsActive

Permissions:
- FULL ACCESS to everything in their clinic
- Manage Staff (hire/fire)
- View Financial Reports
- Manage Clinic Settings
- Manage Inventory
- View All Data
```

---

### **3. Support Staff**

#### **Pharmacist**

```csharp
Table: Pharmacists
Fields:
- UserId (FK to Users) - One-to-one
- LicenseNumber
- Specialization
- CanDispensePrescriptions

Permissions:
- View Prescriptions
- Dispense Medicines
- Manage Medicine Inventory
- View Patient Medication History
- Create Medicine Invoices
```

#### **LabTechnician**

```csharp
Table: LabTechnicians
Fields:
- UserId (FK to Users) - One-to-one
- Certification
- Specialization (Blood, Radiology, etc.)
- CanApproveResults

Permissions:
- View Lab Orders
- Enter Lab Results
- Manage Lab Equipment
- Limited Patient Info Access
```

#### **Accountant**

```csharp
Table: Accountants
Fields:
- UserId (FK to Users) - One-to-one
- CertificationNumber
- CanApproveExpenses
- MaxApprovalAmount

Permissions:
- View All Financial Data
- Generate Financial Reports
- Manage Expenses
- Process Payroll
- NO access to medical records
```

---

## 🔐 Multi-Tenancy Implementation

### **Data Isolation Strategy**

```
┌─────────────────────────────────────────────────────────────┐
│ CLINIC A                    │ CLINIC B                      │
├─────────────────────────────┼───────────────────────────────┤
│ Users (ClinicId = A)        │ Users (ClinicId = B)         │
│ - Doctor1                   │ - Doctor3                     │
│ - Receptionist1             │ - Receptionist2               │
│                             │                               │
│ Patients (ClinicId = A)     │ Patients (ClinicId = B)      │
│ - Patient1, Patient2        │ - Patient3, Patient4          │
│                             │                               │
│ Appointments (via Patient)  │ Appointments (via Patient)    │
│ Invoices (ClinicId = A)     │ Invoices (ClinicId = B)      │
│ Medicines (ClinicId = A)    │ Medicines (ClinicId = B)     │
└─────────────────────────────┴───────────────────────────────┘
```

### **Shared Reference Data (No ClinicId)**

- ChronicDiseases (Diabetes, Hypertension, etc.)
- Specializations (Cardiology, Pediatrics, etc.)
- MeasurementAttributes (Blood Pressure, Temperature)
- AppointmentTypes (Consultation, Follow-up, etc.)

### **Clinic-Specific Data (Has ClinicId)**

- Users (Staff)
- Patients
- Appointments
- Invoices
- Payments
- Medicines (Inventory)
- MedicalSupplies (Inventory)
- MedicalServices (Pricing)
- ClinicBranches

---

## 🎫 JWT Token Structure

```json
{
  "sub": "user-id-guid",
  "email": "doctor@clinic.com",
  "name": "Dr. John Smith",
  "ClinicId": "clinic-id-guid",
  "UserType": "Doctor",
  "role": "Doctor",
  "exp": 1234567890,
  "iss": "ClinicManagementAPI",
  "aud": "ClinicManagementClient"
}
```

### **Claims Explanation**:

- `sub`: User ID (from ASP.NET Identity)
- `email`: User's email address
- `name`: User's full name
- `ClinicId`: **CRITICAL** - Used for multi-tenancy filtering
- `UserType`: Enum value (Doctor, Receptionist, etc.)
- `role`: ASP.NET Identity role name
- `exp`: Token expiration timestamp
- `iss`: Token issuer
- `aud`: Token audience

---

## 🔒 Authorization Scenarios

### **Scenario 1: User Access to Patient Data**

```csharp
// Check 1: User belongs to same clinic as patient
if (user.ClinicId != patient.ClinicId)
    return Forbidden("Access denied: Different clinic");

// Check 2: User has required role
if (!user.IsInRole("Doctor") && !user.IsInRole("Nurse"))
    return Forbidden("Access denied: Insufficient permissions");

// Check 3: Resource-specific permissions (optional)
if (medicalRecord.DoctorId != user.DoctorId && !user.IsInRole("ClinicOwner"))
    return Forbidden("Access denied: Not your patient");
```

### **Scenario 2: Query Filtering (Automatic)**

```csharp
// EF Core Named Query Filters (Applied Automatically)
builder.Entity<Patient>()
    .HasQueryFilter("TenantFilter",
        p => _currentUserService.ClinicId == null ||
             p.ClinicId == _currentUserService.ClinicId);

// When querying:
var patients = await _context.Patients.ToListAsync();
// SQL: SELECT * FROM Patients WHERE ClinicId = @currentClinicId
```

### **Scenario 3: Role-Based Access**

```csharp
// Receptionist can create appointments but NOT view medical records
[Authorize(Roles = "Receptionist, Doctor, ClinicOwner")]
public async Task<IActionResult> CreateAppointment() { }

// Only doctors can prescribe medicine
[Authorize(Roles = "Doctor")]
public async Task<IActionResult> PrescribeMedicine() { }

// Only clinic owner can view financial reports
[Authorize(Roles = "ClinicOwner")]
public async Task<IActionResult> GetFinancialReport() { }
```

---

## 📊 Database Schema

### **User Entity (Base)**

```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Email NVARCHAR(256) NOT NULL,
    UserName NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    ClinicId UNIQUEIDENTIFIER NOT NULL, -- REQUIRED
    UserType INT NOT NULL, -- Enum
    ProfileImageUrl NVARCHAR(500),
    PhoneNumber NVARCHAR(20),
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX),
    -- ASP.NET Identity fields...

    CONSTRAINT FK_Users_Clinics FOREIGN KEY (ClinicId)
        REFERENCES Clinics(Id) ON DELETE RESTRICT
);

CREATE INDEX IX_Users_ClinicId ON Users(ClinicId);
CREATE INDEX IX_Users_UserType ON Users(UserType);
```

### **Doctor Entity (Type-Specific)**

```sql
CREATE TABLE Doctors (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL UNIQUE, -- One-to-one
    SpecializationId UNIQUEIDENTIFIER NOT NULL,
    LicenseNumber NVARCHAR(100),
    YearsOfExperience SMALLINT,
    ConsultationFee DECIMAL(18,2),
    AvailableForEmergency BIT NOT NULL DEFAULT 0,
    Biography NVARCHAR(2000),
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2,

    CONSTRAINT FK_Doctors_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Doctors_Specializations FOREIGN KEY (SpecializationId)
        REFERENCES Specializations(Id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IX_Doctors_UserId ON Doctors(UserId);
CREATE INDEX IX_Doctors_SpecializationId ON Doctors(SpecializationId);
CREATE INDEX IX_Doctors_LicenseNumber ON Doctors(LicenseNumber);
```

### **Similar Structure for Other User Types**

- Receptionists
- Nurses
- ClinicOwners
- Pharmacists
- LabTechnicians
- Accountants

---

## 🚀 Usage Examples

### **Example 1: Register a New Doctor**

```csharp
// Step 1: Create User account
var user = new User
{
    Email = "doctor@clinic.com",
    UserName = "doctor@clinic.com",
    FirstName = "John",
    LastName = "Smith",
    ClinicId = clinicId, // REQUIRED
    UserType = UserType.Doctor
};

await _userManager.CreateAsync(user, password);

// Step 2: Assign Role
await _userManager.AddToRoleAsync(user, Roles.Doctor);

// Step 3: Create Doctor record
var doctor = new Doctor
{
    UserId = user.Id,
    SpecializationId = specializationId,
    LicenseNumber = "MD12345",
    YearsOfExperience = 10,
    ConsultationFee = 100.00m,
    AvailableForEmergency = true
};

await _context.Doctors.AddAsync(doctor);
await _context.SaveChangesAsync();
```

### **Example 2: Login and Generate JWT**

```csharp
// Step 1: Validate credentials
var user = await _userManager.FindByEmailAsync(email);
var isValid = await _userManager.CheckPasswordAsync(user, password);

// Step 2: Get roles
var roles = await _userManager.GetRolesAsync(user);

// Step 3: Generate JWT with ClinicId
var token = _tokenService.GenerateAccessToken(user, roles);
// Token will include: UserId, Email, ClinicId, UserType, Roles

// Step 4: Return token to client
return Ok(new { accessToken = token });
```

### **Example 3: Authorize Request**

```csharp
[Authorize(Roles = "Doctor")]
[HttpGet("patients/{patientId}")]
public async Task<IActionResult> GetPatient(Guid patientId)
{
    // Get current user's ClinicId from JWT
    var clinicId = _currentUserService.ClinicId;

    // Query automatically filtered by ClinicId (via query filter)
    var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);

    if (patient == null)
        return NotFound(); // Either doesn't exist or different clinic

    return Ok(patient);
}
```

---

## ✅ Benefits of This Architecture

1. **Simplicity**: One user = one clinic = simple logic
2. **Performance**: Small JWT tokens, fast queries
3. **Security**: Automatic tenant isolation via query filters
4. **Scalability**: Easy to shard by ClinicId if needed
5. **Flexibility**: Easy to add new user types
6. **Type Safety**: Strongly typed entities for each user type
7. **Maintainability**: Clear separation of concerns

---

## 🎯 Best Practices

1. **Always check ClinicId** in authorization logic
2. **Use query filters** for automatic tenant isolation
3. **Store ClinicId in JWT** for performance
4. **Use ASP.NET Identity Roles** for role-based authorization
5. **Create type-specific records** when registering users
6. **Validate user type** matches role assignment
7. **Use constants** for role names (avoid typos)

---

## 📝 Notes

- User **cannot change clinics** (ClinicId is immutable after creation)
- User **cannot have multiple types** (one UserType per user)
- **ClinicOwner** has full access to all clinic data
- **Shared reference data** has no ClinicId (e.g., ChronicDiseases)
- **Query filters** automatically apply ClinicId filtering
- **JWT tokens** are stateless and contain all necessary claims

---

## 🔮 Future Enhancements

If needed in the future, you can:

1. Add more user types (e.g., Radiologist, Anesthesiologist)
2. Implement role hierarchy (e.g., ClinicOwner inherits all permissions)
3. Add claims-based authorization for fine-grained permissions
4. Implement audit logging for all actions
5. Add user activity tracking per clinic

---

**Last Updated**: 2026-02-06
**Architecture Version**: 1.0
