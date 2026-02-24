-- =============================================
-- Migration: 004_EnumConversion.sql
-- Description: Convert string-based status fields to INT enums
-- Date: 2026-02-24
-- =============================================

-- This migration converts string status columns to INT to match C# enum types
-- Order of operations:
-- 1. Add temporary INT column
-- 2. Migrate data using CASE statements
-- 3. Drop old string column
-- 4. Rename new column to original name
-- 5. Add constraints and defaults

BEGIN TRANSACTION;

-- =============================================
-- SECTION 1: Staff.Status (string → StaffStatus enum)
-- =============================================
PRINT 'Converting Staff.Status to INT enum...';

-- Add temporary column
ALTER TABLE Staff ADD StatusTemp INT NULL;

-- Migrate existing data
UPDATE Staff 
SET StatusTemp = CASE 
    WHEN Status = 'Active' THEN 0
    WHEN Status = 'OnLeave' THEN 1
    WHEN Status = 'Suspended' THEN 2
    WHEN Status = 'Terminated' THEN 3
    WHEN Status = 'Resigned' THEN 4
    ELSE 0 -- Default to Active for any unknown values
END;

-- Drop old column
ALTER TABLE Staff DROP COLUMN Status;

-- Rename temp column
EXEC sp_rename 'Staff.StatusTemp', 'Status', 'COLUMN';

-- Make NOT NULL with default
ALTER TABLE Staff ALTER COLUMN Status INT NOT NULL;
ALTER TABLE Staff ADD CONSTRAINT DF_Staff_Status DEFAULT 0 FOR Status;

-- Add check constraint
ALTER TABLE Staff ADD CONSTRAINT CK_Staff_Status 
    CHECK (Status IN (0, 1, 2, 3, 4));

PRINT 'Staff.Status conversion complete.';

-- =============================================
-- SECTION 2: ClinicSubscriptions.Status (string → SubscriptionStatus enum)
-- =============================================
PRINT 'Converting ClinicSubscriptions.Status to INT enum...';

-- Add temporary column
ALTER TABLE ClinicSubscriptions ADD StatusTemp INT NULL;

-- Migrate existing data
UPDATE ClinicSubscriptions 
SET StatusTemp = CASE 
    WHEN Status = 'Trial' THEN 0
    WHEN Status = 'Active' THEN 1
    WHEN Status = 'PastDue' THEN 2
    WHEN Status = 'Cancelled' THEN 3
    WHEN Status = 'Expired' THEN 4
    ELSE 0 -- Default to Trial for any unknown values
END;

-- Drop old column
ALTER TABLE ClinicSubscriptions DROP COLUMN Status;

-- Rename temp column
EXEC sp_rename 'ClinicSubscriptions.StatusTemp', 'Status', 'COLUMN';

-- Make NOT NULL with default
ALTER TABLE ClinicSubscriptions ALTER COLUMN Status INT NOT NULL;
ALTER TABLE ClinicSubscriptions ADD CONSTRAINT DF_ClinicSubscriptions_Status DEFAULT 0 FOR Status;

-- Add check constraint
ALTER TABLE ClinicSubscriptions ADD CONSTRAINT CK_ClinicSubscriptions_Status 
    CHECK (Status IN (0, 1, 2, 3, 4));

PRINT 'ClinicSubscriptions.Status conversion complete.';

-- =============================================
-- SECTION 3: SubscriptionPayments.Status (string → SubscriptionPaymentStatus enum)
-- =============================================
PRINT 'Converting SubscriptionPayments.Status to INT enum...';

-- Add temporary column
ALTER TABLE SubscriptionPayments ADD StatusTemp INT NULL;

-- Migrate existing data
UPDATE SubscriptionPayments 
SET StatusTemp = CASE 
    WHEN Status = 'Pending' THEN 0
    WHEN Status = 'Completed' THEN 1
    WHEN Status = 'Failed' THEN 2
    WHEN Status = 'Refunded' THEN 3
    ELSE 0 -- Default to Pending for any unknown values
END;

-- Drop old column
ALTER TABLE SubscriptionPayments DROP COLUMN Status;

-- Rename temp column
EXEC sp_rename 'SubscriptionPayments.StatusTemp', 'Status', 'COLUMN';

-- Make NOT NULL with default
ALTER TABLE SubscriptionPayments ALTER COLUMN Status INT NOT NULL;
ALTER TABLE SubscriptionPayments ADD CONSTRAINT DF_SubscriptionPayments_Status DEFAULT 0 FOR Status;

-- Add check constraint
ALTER TABLE SubscriptionPayments ADD CONSTRAINT CK_SubscriptionPayments_Status 
    CHECK (Status IN (0, 1, 2, 3));

PRINT 'SubscriptionPayments.Status conversion complete.';

-- =============================================
-- SECTION 4: Notifications.Type (string → NotificationType enum)
-- =============================================
PRINT 'Converting Notifications.Type to INT enum...';

-- Add temporary column
ALTER TABLE Notifications ADD TypeTemp INT NULL;

-- Migrate existing data
UPDATE Notifications 
SET TypeTemp = CASE 
    WHEN Type = 'Info' THEN 0
    WHEN Type = 'Warning' THEN 1
    WHEN Type = 'Error' THEN 2
    WHEN Type = 'Success' THEN 3
    ELSE 0 -- Default to Info for any unknown values
END;

-- Drop old column
ALTER TABLE Notifications DROP COLUMN Type;

-- Rename temp column
EXEC sp_rename 'Notifications.TypeTemp', 'Type', 'COLUMN';

-- Make NOT NULL with default
ALTER TABLE Notifications ALTER COLUMN Type INT NOT NULL;
ALTER TABLE Notifications ADD CONSTRAINT DF_Notifications_Type DEFAULT 0 FOR Type;

-- Add check constraint
ALTER TABLE Notifications ADD CONSTRAINT CK_Notifications_Type 
    CHECK (Type IN (0, 1, 2, 3));

PRINT 'Notifications.Type conversion complete.';

-- =============================================
-- SECTION 5: EmailQueue.Status (string → EmailQueueStatus enum)
-- =============================================
PRINT 'Converting EmailQueue.Status to INT enum...';

-- Add temporary column
ALTER TABLE EmailQueue ADD StatusTemp INT NULL;

-- Migrate existing data
UPDATE EmailQueue 
SET StatusTemp = CASE 
    WHEN Status = 'Pending' THEN 0
    WHEN Status = 'Sending' THEN 1
    WHEN Status = 'Sent' THEN 2
    WHEN Status = 'Failed' THEN 3
    ELSE 0 -- Default to Pending for any unknown values
END;

-- Drop old column
ALTER TABLE EmailQueue DROP COLUMN Status;

-- Rename temp column
EXEC sp_rename 'EmailQueue.StatusTemp', 'Status', 'COLUMN';

-- Make NOT NULL with default
ALTER TABLE EmailQueue ALTER COLUMN Status INT NOT NULL;
ALTER TABLE EmailQueue ADD CONSTRAINT DF_EmailQueue_Status DEFAULT 0 FOR Status;

-- Add check constraint
ALTER TABLE EmailQueue ADD CONSTRAINT CK_EmailQueue_Status 
    CHECK (Status IN (0, 1, 2, 3));

PRINT 'EmailQueue.Status conversion complete.';

-- =============================================
-- SECTION 6: Add indexes for better query performance
-- =============================================
PRINT 'Adding performance indexes...';

-- Index for filtering staff by status
CREATE NONCLUSTERED INDEX IX_Staff_Status 
    ON Staff(Status) 
    INCLUDE (UserId, ClinicId, IsActive);

-- Index for filtering subscriptions by status
CREATE NONCLUSTERED INDEX IX_ClinicSubscriptions_Status 
    ON ClinicSubscriptions(Status) 
    INCLUDE (ClinicId, StartDate, EndDate);

-- Index for filtering payments by status
CREATE NONCLUSTERED INDEX IX_SubscriptionPayments_Status 
    ON SubscriptionPayments(Status) 
    INCLUDE (SubscriptionId, PaymentDate, Amount);

-- Index for filtering notifications by type and read status
CREATE NONCLUSTERED INDEX IX_Notifications_Type_IsRead 
    ON Notifications(Type, IsRead) 
    INCLUDE (UserId, CreatedAt);

-- Index for email queue processing
CREATE NONCLUSTERED INDEX IX_EmailQueue_Status_Priority 
    ON EmailQueue(Status, Priority, ScheduledFor) 
    INCLUDE (CreatedAt, Attempts);

PRINT 'Performance indexes added.';

-- =============================================
-- SECTION 7: Verification
-- =============================================
PRINT 'Verifying conversions...';

-- Check for any NULL values (should be none)
DECLARE @NullCount INT = 0;

SELECT @NullCount = COUNT(*) FROM Staff WHERE Status IS NULL;
IF @NullCount > 0 RAISERROR('Found NULL values in Staff.Status', 16, 1);

SELECT @NullCount = COUNT(*) FROM ClinicSubscriptions WHERE Status IS NULL;
IF @NullCount > 0 RAISERROR('Found NULL values in ClinicSubscriptions.Status', 16, 1);

SELECT @NullCount = COUNT(*) FROM SubscriptionPayments WHERE Status IS NULL;
IF @NullCount > 0 RAISERROR('Found NULL values in SubscriptionPayments.Status', 16, 1);

SELECT @NullCount = COUNT(*) FROM Notifications WHERE Type IS NULL;
IF @NullCount > 0 RAISERROR('Found NULL values in Notifications.Type', 16, 1);

SELECT @NullCount = COUNT(*) FROM EmailQueue WHERE Status IS NULL;
IF @NullCount > 0 RAISERROR('Found NULL values in EmailQueue.Status', 16, 1);

PRINT 'All conversions verified successfully.';

COMMIT TRANSACTION;

PRINT '✅ Migration 004_EnumConversion.sql completed successfully!';
PRINT '';
PRINT 'Summary:';
PRINT '- Staff.Status: string → INT (StaffStatus enum)';
PRINT '- ClinicSubscriptions.Status: string → INT (SubscriptionStatus enum)';
PRINT '- SubscriptionPayments.Status: string → INT (SubscriptionPaymentStatus enum)';
PRINT '- Notifications.Type: string → INT (NotificationType enum)';
PRINT '- EmailQueue.Status: string → INT (EmailQueueStatus enum)';
PRINT '- Added 5 check constraints';
PRINT '- Added 5 performance indexes';
PRINT '';
PRINT '⚠️  BREAKING CHANGE: API responses now return numeric enum values instead of strings';
PRINT '   Frontend enums have been updated to match (numeric values)';
