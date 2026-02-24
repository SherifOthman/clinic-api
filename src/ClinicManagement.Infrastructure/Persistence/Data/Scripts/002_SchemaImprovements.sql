-- =============================================
-- Clinic Management System - Schema Improvements
-- Migration Version: 002
-- Date: 2024
-- Description: Comprehensive database schema improvements for data integrity,
--              scalability, audit trails, and subscription management
-- =============================================
-- Requirements: US-1, US-2, US-3, US-4, US-5, US-6, US-7, US-9, US-10
-- =============================================

-- =============================================
-- PHASE 1: CREATE NEW TABLES
-- =============================================
-- This phase creates 8 new tables for enhanced functionality:
-- - UserRoleHistory: Audit trail for role changes
-- - ClinicSubscriptions: Subscription lifecycle management
-- - SubscriptionPayments: Payment tracking
-- - ClinicUsageMetrics: Usage tracking for plan limits
-- - Notifications: User notification system
-- - EmailQueue: Email delivery queue
-- - DoctorSpecializations: Multi-specialization support
-- - StaffBranches: Staff assignment to branches
-- =============================================

-- UserRoleHistory table - Audit trail for role changes (US-3)
CREATE TABLE UserRoleHistory (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    Action NVARCHAR(20) NOT NULL, -- 'Added' or 'Removed'
    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ChangedBy UNIQUEIDENTIFIER NOT NULL,
    Reason NVARCHAR(500),
    CONSTRAINT FK_UserRoleHistory_UserId FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserRoleHistory_RoleId FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_UserRoleHistory_ChangedBy FOREIGN KEY (ChangedBy) REFERENCES Users(Id)
);

CREATE INDEX IX_UserRoleHistory_UserId ON UserRoleHistory(UserId);
CREATE INDEX IX_UserRoleHistory_ChangedAt ON UserRoleHistory(ChangedAt DESC);

-- ClinicSubscriptions table - Subscription lifecycle management (US-4)
CREATE TABLE ClinicSubscriptions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- 'Trial', 'Active', 'PastDue', 'Cancelled', 'Expired'
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2,
    TrialEndDate DATETIME2,
    AutoRenew BIT NOT NULL DEFAULT 1,
    CancellationReason NVARCHAR(500),
    CancelledAt DATETIME2,
    CancelledBy UNIQUEIDENTIFIER,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_ClinicSubscriptions_ClinicId FOREIGN KEY (ClinicId) REFERENCES Clinics(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ClinicSubscriptions_SubscriptionPlanId FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id),
    CONSTRAINT FK_ClinicSubscriptions_CancelledBy FOREIGN KEY (CancelledBy) REFERENCES Users(Id)
);

CREATE INDEX IX_ClinicSubscriptions_ClinicId_Status ON ClinicSubscriptions(ClinicId, Status);
CREATE INDEX IX_ClinicSubscriptions_Status_EndDate ON ClinicSubscriptions(Status, EndDate);

-- SubscriptionPayments table - Payment tracking (US-4)
CREATE TABLE SubscriptionPayments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
    PaymentDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- 'Pending', 'Completed', 'Failed', 'Refunded'
    PaymentMethod NVARCHAR(50), -- 'CreditCard', 'BankTransfer', 'PayPal', etc.
    TransactionId NVARCHAR(200),
    PaymentGateway NVARCHAR(50),
    FailureReason NVARCHAR(500),
    RefundedAt DATETIME2,
    RefundAmount DECIMAL(18,2),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_SubscriptionPayments_SubscriptionId FOREIGN KEY (SubscriptionId) REFERENCES ClinicSubscriptions(Id) ON DELETE CASCADE
);

CREATE INDEX IX_SubscriptionPayments_SubscriptionId ON SubscriptionPayments(SubscriptionId);
CREATE INDEX IX_SubscriptionPayments_PaymentDate ON SubscriptionPayments(PaymentDate DESC);
CREATE INDEX IX_SubscriptionPayments_Status ON SubscriptionPayments(Status);

-- ClinicUsageMetrics table - Usage tracking for plan limits (US-5)
CREATE TABLE ClinicUsageMetrics (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicId UNIQUEIDENTIFIER NOT NULL,
    MetricDate DATE NOT NULL,
    ActiveStaffCount INT NOT NULL DEFAULT 0,
    NewPatientsCount INT NOT NULL DEFAULT 0,
    TotalPatientsCount INT NOT NULL DEFAULT 0,
    AppointmentsCount INT NOT NULL DEFAULT 0,
    InvoicesCount INT NOT NULL DEFAULT 0,
    StorageUsedGB DECIMAL(10,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_ClinicUsageMetrics_ClinicId FOREIGN KEY (ClinicId) REFERENCES Clinics(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ClinicUsageMetrics_ClinicId_MetricDate UNIQUE (ClinicId, MetricDate)
);

CREATE INDEX IX_ClinicUsageMetrics_ClinicId_MetricDate ON ClinicUsageMetrics(ClinicId, MetricDate DESC);

-- Notifications table - User notification system (US-9)
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- 'Info', 'Warning', 'Error', 'Success'
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    ActionUrl NVARCHAR(500),
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2,
    CONSTRAINT FK_Notifications_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Notifications_UserId_IsRead_CreatedAt ON Notifications(UserId, IsRead, CreatedAt DESC);

-- EmailQueue table - Email delivery queue (US-9)
CREATE TABLE EmailQueue (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ToEmail NVARCHAR(256) NOT NULL,
    ToName NVARCHAR(200),
    Subject NVARCHAR(500) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    IsHtml BIT NOT NULL DEFAULT 1,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Sending', 'Sent', 'Failed'
    Priority INT NOT NULL DEFAULT 5, -- 1=Highest, 10=Lowest
    Attempts INT NOT NULL DEFAULT 0,
    MaxAttempts INT NOT NULL DEFAULT 3,
    SentAt DATETIME2,
    ErrorMessage NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ScheduledFor DATETIME2,
    CONSTRAINT CK_EmailQueue_Priority CHECK (Priority BETWEEN 1 AND 10)
);

CREATE INDEX IX_EmailQueue_Status_CreatedAt ON EmailQueue(Status, CreatedAt);
CREATE INDEX IX_EmailQueue_Status_Priority ON EmailQueue(Status, Priority);

-- DoctorSpecializations table - Multi-specialization support (US-10)
CREATE TABLE DoctorSpecializations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DoctorProfileId UNIQUEIDENTIFIER NOT NULL,
    SpecializationId UNIQUEIDENTIFIER NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    YearsOfExperience INT NOT NULL DEFAULT 0,
    CertificationNumber NVARCHAR(100),
    CertificationDate DATE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_DoctorSpecializations_DoctorProfileId FOREIGN KEY (DoctorProfileId) REFERENCES DoctorProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DoctorSpecializations_SpecializationId FOREIGN KEY (SpecializationId) REFERENCES Specializations(Id),
    CONSTRAINT UQ_DoctorSpecializations_DoctorProfileId_SpecializationId UNIQUE (DoctorProfileId, SpecializationId)
);

CREATE UNIQUE INDEX IX_DoctorSpecializations_Primary ON DoctorSpecializations(DoctorProfileId)
    WHERE IsPrimary = 1;
CREATE INDEX IX_DoctorSpecializations_SpecializationId ON DoctorSpecializations(SpecializationId);

-- StaffBranches table - Staff assignment to branches (US-2)
CREATE TABLE StaffBranches (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    StaffId UNIQUEIDENTIFIER NOT NULL,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    IsPrimaryBranch BIT NOT NULL DEFAULT 0,
    StartDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EndDate DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_StaffBranches_StaffId FOREIGN KEY (StaffId) REFERENCES Staff(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StaffBranches_ClinicBranchId FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id),
    CONSTRAINT UQ_StaffBranches_StaffId_ClinicBranchId UNIQUE (StaffId, ClinicBranchId)
);

CREATE UNIQUE INDEX IX_StaffBranches_Primary ON StaffBranches(StaffId)
    WHERE IsPrimaryBranch = 1 AND IsActive = 1;
CREATE INDEX IX_StaffBranches_ClinicBranchId ON StaffBranches(ClinicBranchId);

GO

-- =============================================
-- PHASE 2: ADD COLUMNS TO EXISTING TABLES
-- =============================================
-- This phase adds new columns to existing tables to support:
-- - Employment tracking (Staff)
-- - Security enhancements (Users)
-- - Subscription dates (Clinics)
-- - License tracking (DoctorProfiles)
-- - Plan versioning (SubscriptionPlans)
-- - Timestamps (MedicalVisits)
-- =============================================

-- Add employment tracking columns to Staff table (US-2)
ALTER TABLE Staff ADD IsPrimaryClinic BIT NOT NULL DEFAULT 0;
ALTER TABLE Staff ADD Status NVARCHAR(20) NOT NULL DEFAULT 'Active'; -- 'Active', 'OnLeave', 'Suspended', 'Terminated', 'Resigned'
ALTER TABLE Staff ADD StatusChangedAt DATETIME2;
ALTER TABLE Staff ADD StatusChangedBy UNIQUEIDENTIFIER;
ALTER TABLE Staff ADD StatusReason NVARCHAR(500);
ALTER TABLE Staff ADD TerminationDate DATETIME2;
ALTER TABLE Staff ADD CONSTRAINT FK_Staff_StatusChangedBy FOREIGN KEY (StatusChangedBy) REFERENCES Users(Id);

-- Add security columns to Users table (US-7)
ALTER TABLE Users ADD FailedLoginAttempts INT NOT NULL DEFAULT 0;
ALTER TABLE Users ADD LockoutEndDate DATETIME2;
ALTER TABLE Users ADD LastLoginAt DATETIME2;
ALTER TABLE Users ADD LastPasswordChangeAt DATETIME2;

-- Modify PasswordHash column from NVARCHAR(MAX) to fixed length (US-7)
-- Note: This requires data migration if existing hashes are longer than 200 characters
ALTER TABLE Users ALTER COLUMN PasswordHash NVARCHAR(200) NOT NULL;

-- Add subscription columns to Clinics table (US-4)
ALTER TABLE Clinics ADD SubscriptionStartDate DATETIME2;
ALTER TABLE Clinics ADD SubscriptionEndDate DATETIME2;
ALTER TABLE Clinics ADD TrialEndDate DATETIME2;
ALTER TABLE Clinics ADD BillingEmail NVARCHAR(256);

-- Add license tracking columns to DoctorProfiles table (US-2)
ALTER TABLE DoctorProfiles ADD LicenseNumber NVARCHAR(100);
ALTER TABLE DoctorProfiles ADD LicenseExpiryDate DATE;
ALTER TABLE DoctorProfiles ADD Bio NVARCHAR(2000);
ALTER TABLE DoctorProfiles ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
ALTER TABLE DoctorProfiles ADD UpdatedAt DATETIME2;

-- Add versioning columns to SubscriptionPlans table (US-4)
ALTER TABLE SubscriptionPlans ADD Version INT NOT NULL DEFAULT 1;
ALTER TABLE SubscriptionPlans ADD EffectiveDate DATETIME2 NOT NULL DEFAULT GETUTCDATE();
ALTER TABLE SubscriptionPlans ADD ExpiryDate DATETIME2;

-- Add timestamp columns to MedicalVisits table (US-6)
ALTER TABLE MedicalVisits ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
ALTER TABLE MedicalVisits ADD UpdatedAt DATETIME2;

GO

-- =============================================
-- PHASE 3: DATA CLEANUP AND UNIQUE CONSTRAINTS
-- =============================================
-- This phase cleans up duplicate data and adds unique constraints
-- to prevent future duplicates
-- =============================================

-- Clean up duplicate Staff records (US-1)
-- Soft delete duplicates, keeping the oldest record
WITH DuplicateStaff AS (
    SELECT UserId, ClinicId, MIN(Id) as KeepId
    FROM Staff
    WHERE IsDeleted = 0
    GROUP BY UserId, ClinicId
    HAVING COUNT(*) > 1
)
UPDATE Staff SET IsDeleted = 1
WHERE Id NOT IN (SELECT KeepId FROM DuplicateStaff)
AND EXISTS (
    SELECT 1 FROM DuplicateStaff 
    WHERE Staff.UserId = DuplicateStaff.UserId 
    AND Staff.ClinicId = DuplicateStaff.ClinicId
);

-- Add unique constraint to Staff table (US-1, US-8)
ALTER TABLE Staff ADD CONSTRAINT UQ_Staff_UserId_ClinicId UNIQUE (UserId, ClinicId);

-- Add filtered unique index for IsPrimaryClinic (US-2)
CREATE UNIQUE INDEX IX_Staff_Primary ON Staff(UserId)
    WHERE IsPrimaryClinic = 1 AND IsDeleted = 0;

-- Clean up duplicate Patient codes (US-1)
-- Append GUID to duplicate patient codes
WITH DuplicatePatients AS (
    SELECT ClinicId, PatientCode, MIN(Id) as KeepId
    FROM Patients
    WHERE IsDeleted = 0 AND PatientCode IS NOT NULL
    GROUP BY ClinicId, PatientCode
    HAVING COUNT(*) > 1
)
UPDATE Patients 
SET PatientCode = PatientCode + '_' + CAST(NEWID() AS NVARCHAR(36))
WHERE Id NOT IN (SELECT KeepId FROM DuplicatePatients)
AND EXISTS (
    SELECT 1 FROM DuplicatePatients 
    WHERE Patients.ClinicId = DuplicatePatients.ClinicId 
    AND Patients.PatientCode = DuplicatePatients.PatientCode
);

-- Add unique constraint to Patients table (US-1)
ALTER TABLE Patients ADD CONSTRAINT UQ_Patients_ClinicId_PatientCode UNIQUE (ClinicId, PatientCode);

-- Add unique constraint to DoctorProfiles table (US-1)
ALTER TABLE DoctorProfiles ADD CONSTRAINT UQ_DoctorProfiles_StaffId UNIQUE (StaffId);

-- Add unique constraint to ClinicBranches table (US-1)
ALTER TABLE ClinicBranches ADD CONSTRAINT UQ_ClinicBranches_ClinicId_Name UNIQUE (ClinicId, Name);

-- Add filtered unique index for IsMainBranch (US-1)
CREATE UNIQUE INDEX IX_ClinicBranches_MainBranch ON ClinicBranches(ClinicId)
    WHERE IsMainBranch = 1 AND IsDeleted = 0;

-- Add unique constraint to Specializations table (US-1)
ALTER TABLE Specializations ADD CONSTRAINT UQ_Specializations_NameEn UNIQUE (NameEn);

-- Add unique constraint to SubscriptionPlans table (US-4)
ALTER TABLE SubscriptionPlans ADD CONSTRAINT UQ_SubscriptionPlans_Name_Version UNIQUE (Name, Version);

GO

-- =============================================
-- PHASE 4: MIGRATE DOCTORPROFILES TO DOCTORSPECIALIZATIONS
-- =============================================
-- This phase migrates existing specializations from DoctorProfiles
-- to the new DoctorSpecializations table for multi-specialization support
-- =============================================

-- Migrate existing specializations to DoctorSpecializations (US-10)
INSERT INTO DoctorSpecializations (Id, DoctorProfileId, SpecializationId, IsPrimary, YearsOfExperience, CreatedAt)
SELECT
    NEWID(),
    Id as DoctorProfileId,
    SpecializationId,
    1 as IsPrimary,
    ISNULL(YearsOfExperience, 0),
    GETUTCDATE()
FROM DoctorProfiles
WHERE SpecializationId IS NOT NULL;

-- Verification: Count records to ensure migration completed
-- SELECT COUNT(*) as DoctorProfilesWithSpecialization FROM DoctorProfiles WHERE SpecializationId IS NOT NULL;
-- SELECT COUNT(*) as DoctorSpecializationsWithPrimary FROM DoctorSpecializations WHERE IsPrimary = 1;
-- Note: Both counts should match

-- Drop old columns from DoctorProfiles (US-10)
-- IMPORTANT: Run these commands AFTER manual verification of the migration
-- ALTER TABLE DoctorProfiles DROP COLUMN SpecializationId;
-- ALTER TABLE DoctorProfiles DROP COLUMN YearsOfExperience;

-- =============================================
-- PHASE 5: CREATE INITIAL SUBSCRIPTIONS
-- =============================================
-- This phase creates ClinicSubscriptions records for existing clinics
-- =============================================

-- Create subscription records for existing clinics (US-4)
INSERT INTO ClinicSubscriptions (Id, ClinicId, SubscriptionPlanId, Status, StartDate, EndDate, TrialEndDate, AutoRenew, CreatedAt)
SELECT
    NEWID(),
    Id as ClinicId,
    SubscriptionPlanId,
    CASE
        WHEN TrialEndDate > GETUTCDATE() THEN 'Trial'
        WHEN SubscriptionEndDate > GETUTCDATE() THEN 'Active'
        WHEN SubscriptionEndDate IS NULL THEN 'Active'
        ELSE 'Expired'
    END as Status,
    ISNULL(SubscriptionStartDate, CreatedAt) as StartDate,
    SubscriptionEndDate,
    TrialEndDate,
    1 as AutoRenew,
    GETUTCDATE()
FROM Clinics
WHERE IsDeleted = 0;

GO

-- =============================================
-- PHASE 6: ADD COMPOSITE INDEXES FOR PERFORMANCE
-- =============================================
-- This phase adds composite indexes to improve query performance
-- on high-traffic tables
-- =============================================

-- RefreshTokens composite index (US-6)
CREATE INDEX IX_RefreshTokens_UserId_ExpiryTime_IsRevoked
    ON RefreshTokens(UserId, ExpiryTime, IsRevoked)
    WHERE IsRevoked = 0;

-- Appointments composite indexes (US-6)
CREATE INDEX IX_Appointments_DoctorId_AppointmentDate_Status
    ON Appointments(DoctorId, AppointmentDate, Status);

CREATE INDEX IX_Appointments_PatientId_AppointmentDate
    ON Appointments(PatientId, AppointmentDate DESC);

CREATE INDEX IX_Appointments_ClinicBranchId_AppointmentDate_Status
    ON Appointments(ClinicBranchId, AppointmentDate, Status);

-- Invoices composite indexes (US-6)
CREATE INDEX IX_Invoices_ClinicId_IssuedDate_Status
    ON Invoices(ClinicId, IssuedDate DESC, Status);

CREATE INDEX IX_Invoices_PatientId_IssuedDate
    ON Invoices(PatientId, IssuedDate DESC);

CREATE INDEX IX_Invoices_Status_DueDate
    ON Invoices(Status, DueDate);

-- Patients indexes (US-6)
CREATE INDEX IX_Patients_ClinicId_IsDeleted_CreatedAt
    ON Patients(ClinicId, IsDeleted, CreatedAt DESC)
    WHERE IsDeleted = 0;

-- Full-text index for patient name search (US-6)
-- Note: Requires full-text catalog to be created first
-- CREATE FULLTEXT CATALOG FT_Catalog AS DEFAULT;
-- CREATE FULLTEXT INDEX ON Patients(FullName) KEY INDEX PK_Patients;

-- MedicalVisits composite index (US-6)
CREATE INDEX IX_MedicalVisits_PatientId_CreatedAt
    ON MedicalVisits(PatientId, CreatedAt DESC);

-- UserRoles index (US-6)
CREATE INDEX IX_UserRoles_RoleId ON UserRoles(RoleId);

-- =============================================
-- MIGRATION COMPLETE
-- =============================================
-- Summary:
-- - 8 new tables created
-- - 6 existing tables modified with new columns
-- - 7 unique constraints added
-- - 15+ composite indexes added for performance
-- - Data cleanup performed for duplicates
-- - DoctorProfiles migrated to DoctorSpecializations
-- - Initial subscriptions created for existing clinics
-- =============================================
