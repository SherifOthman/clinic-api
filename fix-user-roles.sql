-- Script to assign roles to existing users without deleting data

-- Step 1: Check current state
PRINT 'Current Users:';
SELECT Id, UserName, Email, FirstName, LastName FROM Users;

PRINT '';
PRINT 'Current Roles:';
SELECT Id, Name FROM Roles;

PRINT '';
PRINT 'Current UserRoles:';
SELECT u.Email, r.Name as RoleName
FROM Users u
LEFT JOIN UserRoles ur ON u.Id = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.Id;

-- Step 2: Assign ClinicOwner role to owner@clinic.com if not already assigned
DECLARE @OwnerUserId UNIQUEIDENTIFIER;
DECLARE @ClinicOwnerRoleId UNIQUEIDENTIFIER;

SELECT @OwnerUserId = Id FROM Users WHERE Email = 'owner@clinic.com';
SELECT @ClinicOwnerRoleId = Id FROM Roles WHERE Name = 'ClinicOwner';

IF @OwnerUserId IS NOT NULL AND @ClinicOwnerRoleId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @OwnerUserId AND RoleId = @ClinicOwnerRoleId)
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId) VALUES (@OwnerUserId, @ClinicOwnerRoleId);
        PRINT 'Assigned ClinicOwner role to owner@clinic.com';
    END
    ELSE
    BEGIN
        PRINT 'owner@clinic.com already has ClinicOwner role';
    END
END
ELSE
BEGIN
    PRINT 'Could not find owner@clinic.com or ClinicOwner role';
END

-- Step 3: Assign SuperAdmin role to superadmin@clinic.com if not already assigned
DECLARE @SuperAdminUserId UNIQUEIDENTIFIER;
DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER;

SELECT @SuperAdminUserId = Id FROM Users WHERE Email = 'superadmin@clinic.com';
SELECT @SuperAdminRoleId = Id FROM Roles WHERE Name = 'SuperAdmin';

IF @SuperAdminUserId IS NOT NULL AND @SuperAdminRoleId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @SuperAdminUserId AND RoleId = @SuperAdminRoleId)
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId) VALUES (@SuperAdminUserId, @SuperAdminRoleId);
        PRINT 'Assigned SuperAdmin role to superadmin@clinic.com';
    END
    ELSE
    BEGIN
        PRINT 'superadmin@clinic.com already has SuperAdmin role';
    END
END
ELSE
BEGIN
    PRINT 'Could not find superadmin@clinic.com or SuperAdmin role';
END

-- Step 4: Verify the changes
PRINT '';
PRINT 'Updated UserRoles:';
SELECT u.Email, r.Name as RoleName
FROM Users u
INNER JOIN UserRoles ur ON u.Id = ur.UserId
INNER JOIN Roles r ON ur.RoleId = r.Id
ORDER BY u.Email;
