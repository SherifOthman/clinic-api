using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public StaffRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<Staff?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Staff WHERE Id = @Id AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<Staff>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Staff?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Staff WHERE UserId = @UserId AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<Staff>(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Staff>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Staff WHERE ClinicId = @ClinicId AND IsDeleted = 0 ORDER BY HireDate DESC";
        return await _connection.QueryAsync<Staff>(
            new CommandDefinition(sql, new { ClinicId = clinicId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Staff>> GetActiveByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Staff WHERE ClinicId = @ClinicId AND IsActive = 1 AND IsDeleted = 0 ORDER BY HireDate DESC";
        return await _connection.QueryAsync<Staff>(
            new CommandDefinition(sql, new { ClinicId = clinicId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Staff>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Staff WHERE IsDeleted = 0";
        return await _connection.QueryAsync<Staff>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Staff> AddAsync(Staff entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Staff (
                UserId, ClinicId, IsActive, HireDate, CreatedAt, IsDeleted
            ) VALUES (
                @UserId, @ClinicId, @IsActive, @HireDate, @CreatedAt, @IsDeleted
            );
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        entity.Id = await _connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(Staff entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Staff SET
                UserId = @UserId,
                ClinicId = @ClinicId,
                IsActive = @IsActive,
                HireDate = @HireDate
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE Staff SET IsDeleted = 1 WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
