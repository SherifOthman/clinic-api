using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
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

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM AspNetUsers 
            WHERE Id = @Id";
        
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM AspNetUsers 
            WHERE NormalizedEmail = @NormalizedEmail";
        
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { NormalizedEmail = email.ToUpperInvariant() }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM AspNetUsers 
            WHERE NormalizedUserName = @NormalizedUserName";
        
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { NormalizedUserName = username.ToUpperInvariant() }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM AspNetUsers 
            WHERE NormalizedEmail = @Normalized OR NormalizedUserName = @Normalized";
        
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Normalized = emailOrUsername.ToUpperInvariant() }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT CAST(CASE WHEN EXISTS(
                SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = @NormalizedEmail
            ) THEN 1 ELSE 0 END AS BIT)";
        
        return await _connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { NormalizedEmail = email.ToUpperInvariant() }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT CAST(CASE WHEN EXISTS(
                SELECT 1 FROM AspNetUsers WHERE NormalizedUserName = @NormalizedUserName
            ) THEN 1 ELSE 0 END AS BIT)";
        
        return await _connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { NormalizedUserName = username.ToUpperInvariant() }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM AspNetUsers";
        return await _connection.QueryAsync<User>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO AspNetUsers (
                Id, UserName, NormalizedUserName, Email, NormalizedEmail, 
                EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, 
                LockoutEnd, LockoutEnabled, AccessFailedCount,
                FirstName, LastName, ProfileImageUrl, IsActive, CreatedAt, UpdatedAt
            ) VALUES (
                @Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail,
                @EmailConfirmed, @PasswordHash, @SecurityStamp, @ConcurrencyStamp,
                @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled,
                @LockoutEnd, @LockoutEnabled, @AccessFailedCount,
                @FirstName, @LastName, @ProfileImageUrl, @IsActive, @CreatedAt, @UpdatedAt
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE AspNetUsers SET
                UserName = @UserName,
                NormalizedUserName = @NormalizedUserName,
                Email = @Email,
                NormalizedEmail = @NormalizedEmail,
                EmailConfirmed = @EmailConfirmed,
                PasswordHash = @PasswordHash,
                SecurityStamp = @SecurityStamp,
                ConcurrencyStamp = @ConcurrencyStamp,
                PhoneNumber = @PhoneNumber,
                PhoneNumberConfirmed = @PhoneNumberConfirmed,
                TwoFactorEnabled = @TwoFactorEnabled,
                LockoutEnd = @LockoutEnd,
                LockoutEnabled = @LockoutEnabled,
                AccessFailedCount = @AccessFailedCount,
                FirstName = @FirstName,
                LastName = @LastName,
                ProfileImageUrl = @ProfileImageUrl,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM AspNetUsers WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Staff?> GetStaffByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM Staff 
            WHERE UserId = @UserId AND IsActive = 1";
        
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
}
