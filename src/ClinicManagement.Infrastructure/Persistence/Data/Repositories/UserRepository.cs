using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public UserRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserName, Email, PasswordHash, FirstName, LastName, PhoneNumber, 
                   ProfileImageUrl, IsEmailConfirmed, FailedLoginAttempts, LockoutEndDate, 
                   LastLoginAt, LastPasswordChangeAt
            FROM Users 
            WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserName, Email, PasswordHash, FirstName, LastName, PhoneNumber, 
                   ProfileImageUrl, IsEmailConfirmed, FailedLoginAttempts, LockoutEndDate, 
                   LastLoginAt, LastPasswordChangeAt
            FROM Users 
            WHERE Email = @Email";
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Email = email }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserName, Email, PasswordHash, FirstName, LastName, PhoneNumber, 
                   ProfileImageUrl, IsEmailConfirmed, FailedLoginAttempts, LockoutEndDate, 
                   LastLoginAt, LastPasswordChangeAt
            FROM Users 
            WHERE UserName = @UserName";
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { UserName = username }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserName, Email, PasswordHash, FirstName, LastName, PhoneNumber, 
                   ProfileImageUrl, IsEmailConfirmed, FailedLoginAttempts, LockoutEndDate, 
                   LastLoginAt, LastPasswordChangeAt
            FROM Users 
            WHERE Email = @Value OR UserName = @Value";
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Value = emailOrUsername }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM Users WHERE Email = @Email) THEN 1 ELSE 0 END AS BIT)";
        return await _connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { Email = email }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM Users WHERE UserName = @UserName) THEN 1 ELSE 0 END AS BIT)";
        return await _connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { UserName = username }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserName, Email, PasswordHash, FirstName, LastName, PhoneNumber, 
                   ProfileImageUrl, IsEmailConfirmed, FailedLoginAttempts, LockoutEndDate, 
                   LastLoginAt, LastPasswordChangeAt
            FROM Users";
        return await _connection.QueryAsync<User>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Users (
                Id, UserName, Email, PasswordHash,
                FirstName, LastName, PhoneNumber, ProfileImageUrl, IsEmailConfirmed,
                FailedLoginAttempts, LastPasswordChangeAt
            ) VALUES (
                @Id, @UserName, @Email, @PasswordHash,
                @FirstName, @LastName, @PhoneNumber, @ProfileImageUrl, @IsEmailConfirmed,
                0, GETUTCDATE()
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Users SET
                UserName = @UserName,
                Email = @Email,
                PasswordHash = @PasswordHash,
                FirstName = @FirstName,
                LastName = @LastName,
                PhoneNumber = @PhoneNumber,
                ProfileImageUrl = @ProfileImageUrl,
                IsEmailConfirmed = @IsEmailConfirmed,
                FailedLoginAttempts = @FailedLoginAttempts,
                LockoutEndDate = @LockoutEndDate,
                LastLoginAt = @LastLoginAt,
                LastPasswordChangeAt = @LastPasswordChangeAt
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Users WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Staff?> GetStaffByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Staff WHERE UserId = @UserId AND IsActive = 1";
        return await _connection.QueryFirstOrDefaultAsync<Staff>(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> HasCompletedClinicOnboardingAsync(Guid userId, CancellationToken cancellationToken = default)
    {

            const string sql = @"
                SELECT CAST(CASE WHEN EXISTS(
                    SELECT 1 FROM Clinics 
                    WHERE OwnerUserId = @UserId AND OnboardingCompleted = 1
                ) THEN 1 ELSE 0 END AS BIT)";
            
            return await _connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
     
    }

    public async Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Roles";
        var roles = await _connection.QueryAsync<Role>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
        return roles.ToList();
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT r.Name 
            FROM Roles r
            INNER JOIN UserRoles ur ON r.Id = ur.RoleId
            WHERE ur.UserId = @UserId";
        
        var roles = await _connection.QueryAsync<string>(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
        return roles.ToList();
    }

    public async Task AddUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId, RoleId = roleId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task AddUserRoleAsync(Guid userId, Guid roleId, Guid changedBy, string? reason = null, CancellationToken cancellationToken = default)
    {
        // Add the role
        const string insertRoleSql = "INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
        await _connection.ExecuteAsync(
            new CommandDefinition(insertRoleSql, new { UserId = userId, RoleId = roleId }, _transaction, cancellationToken: cancellationToken));

        // Create audit record
        const string insertAuditSql = @"
            INSERT INTO UserRoleHistory (
                Id, UserId, RoleId, Action, ChangedAt, ChangedBy, Reason
            ) VALUES (
                @Id, @UserId, @RoleId, @Action, GETUTCDATE(), @ChangedBy, @Reason
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(insertAuditSql, new 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId, 
                RoleId = roleId, 
                Action = "Added", 
                ChangedBy = changedBy, 
                Reason = reason 
            }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task RemoveUserRoleAsync(Guid userId, Guid roleId, Guid changedBy, string? reason = null, CancellationToken cancellationToken = default)
    {
        // Remove the role
        const string deleteRoleSql = "DELETE FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId";
        await _connection.ExecuteAsync(
            new CommandDefinition(deleteRoleSql, new { UserId = userId, RoleId = roleId }, _transaction, cancellationToken: cancellationToken));

        // Create audit record
        const string insertAuditSql = @"
            INSERT INTO UserRoleHistory (
                Id, UserId, RoleId, Action, ChangedAt, ChangedBy, Reason
            ) VALUES (
                @Id, @UserId, @RoleId, @Action, GETUTCDATE(), @ChangedBy, @Reason
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(insertAuditSql, new 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId, 
                RoleId = roleId, 
                Action = "Removed", 
                ChangedBy = changedBy, 
                Reason = reason 
            }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task IncrementFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Users SET
                FailedLoginAttempts = FailedLoginAttempts + 1,
                LockoutEndDate = CASE 
                    WHEN FailedLoginAttempts + 1 >= 5 THEN DATEADD(MINUTE, 30, GETUTCDATE())
                    ELSE LockoutEndDate
                END
            WHERE Id = @UserId";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task ResetFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Users SET
                FailedLoginAttempts = 0,
                LockoutEndDate = NULL
            WHERE Id = @UserId";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE Users SET LastLoginAt = GETUTCDATE() WHERE Id = @UserId";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Users SET
                PasswordHash = @PasswordHash,
                LastPasswordChangeAt = GETUTCDATE()
            WHERE Id = @UserId";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId, PasswordHash = passwordHash }, _transaction, cancellationToken: cancellationToken));
    }
}
