-- Script to reset users and reseed with proper roles
-- WARNING: This will delete all users and related data!

-- Step 1: Delete all user-related data (in correct order due to foreign keys)
DELETE FROM UserRoles;
DELETE FROM RefreshTokens;
DELETE FROM UserTokens;
DELETE FROM DoctorSpecializations;
DELETE FROM DoctorProfiles;
DELETE FROM Staff;
DELETE FROM ClinicSubscriptions;
DELETE FROM Clinics;
DELETE FROM Users;

-- Step 2: Verify roles exist
SELECT * FROM Roles;

-- Step 3: The application seeders will recreate the users with roles
-- Restart the application after running this script
-- The SuperAdminSeedService and ClinicOwnerSeedService will run automatically

PRINT 'Database cleaned. Restart the application to reseed users with roles.';
