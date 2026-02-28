-- Test script to check user roles in the database

-- 1. Check all users
SELECT Id, UserName, Email, FirstName, LastName, IsEmailConfirmed
FROM Users;

-- 2. Check all roles
SELECT Id, Name, Description
FROM Roles;

-- 3. Check UserRoles mapping
SELECT 
    u.UserName,
    u.Email,
    r.Name as RoleName
FROM Users u
LEFT JOIN UserRoles ur ON u.Id = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.Id
ORDER BY u.UserName;

-- 4. Check users without roles
SELECT 
    u.Id,
    u.UserName,
    u.Email,
    u.FirstName,
    u.LastName
FROM Users u
LEFT JOIN UserRoles ur ON u.Id = ur.UserId
WHERE ur.UserId IS NULL;

-- 5. Get roles for specific user (replace with actual email)
SELECT r.Name 
FROM Roles r
INNER JOIN UserRoles ur ON r.Id = ur.RoleId
INNER JOIN Users u ON ur.UserId = u.Id
WHERE u.Email = 'owner@clinic.com';
