using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public UserRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Users WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Users WHERE Email = @Email";
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Email = email }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Users WHERE UserName = @UserName";
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { UserName = username }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Users WHERE Email = @Value OR UserName = @Value";
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
        const string sql = "SELECT * FROM Users";
        return await _connection.QueryAsync<User>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Users (
                UserName, Email, PasswordHash,
                FirstName, LastName, PhoneNumber, ProfileImageUrl, IsEmailConfirmed
            ) VALUES (
                @UserName, @Email, @PasswordHash,
                @FirstName, @LastName, @PhoneNumber, @ProfileImageUrl, @IsEmailConfirmed
            );
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        entity.Id = await _connection.ExecuteScalarAsync<int>(
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
                IsEmailConfirmed = @IsEmailConfirmed
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Users WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Staff?> GetStaffByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Staff WHERE UserId = @UserId AND IsActive = 1";
        return await _connection.QueryFirstOrDefaultAsync<Staff>(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> HasCompletedClinicOnboardingAsync(int userId, CancellationToken cancellationToken = default)
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

    public async Task<List<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
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

    public async Task AddUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId, RoleId = roleId }, _transaction, cancellationToken: cancellationToken));
    }
}
