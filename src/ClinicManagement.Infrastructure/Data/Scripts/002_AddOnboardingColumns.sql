-- Add missing columns to ClinicBranches table for onboarding
ALTER TABLE ClinicBranches
ADD Name NVARCHAR(200) NOT NULL DEFAULT '',
    AddressLine NVARCHAR(500) NOT NULL DEFAULT '',
    IsMainBranch BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1;

-- Remove default constraints after adding columns
ALTER TABLE ClinicBranches
ALTER COLUMN Name NVARCHAR(200) NOT NULL;

ALTER TABLE ClinicBranches
ALTER COLUMN AddressLine NVARCHAR(500) NOT NULL;

-- Create indexes
CREATE INDEX IX_ClinicBranches_IsMainBranch ON ClinicBranches(IsMainBranch);
CREATE INDEX IX_ClinicBranches_IsActive ON ClinicBranches(IsActive);
