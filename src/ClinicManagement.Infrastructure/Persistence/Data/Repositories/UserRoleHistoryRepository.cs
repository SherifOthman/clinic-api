using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class UserRoleHistoryRepository : IUserRoleHistoryRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public UserRoleHistoryRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<UserRoleHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM UserRoleHistory WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<UserRoleHistory>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<UserRoleHistory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM UserRoleHistory ORDER BY ChangedAt DESC";
        return await _connection.QueryAsync<UserRoleHistory>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<UserRoleHistory> AddAsync(UserRoleHistory entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO UserRoleHistory (
                Id, UserId, RoleId, Action, ChangedAt, ChangedBy, Reason
            ) VALUES (
                @Id, @UserId, @RoleId, @Action, @ChangedAt, @ChangedBy, @Reason
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(UserRoleHistory entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE UserRoleHistory SET
                UserId = @UserId,
                RoleId = @RoleId,
                Action = @Action,
                ChangedAt = @ChangedAt,
                ChangedBy = @ChangedBy,
                Reason = @Reason
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM UserRoleHistory WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<UserRoleHistory>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM UserRoleHistory WHERE UserId = @UserId ORDER BY ChangedAt DESC";
        return await _connection.QueryAsync<UserRoleHistory>(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<UserRoleHistory>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM UserRoleHistory 
            WHERE ChangedAt >= @Start AND ChangedAt <= @End 
            ORDER BY ChangedAt DESC";
        
        return await _connection.QueryAsync<UserRoleHistory>(
            new CommandDefinition(sql, new { Start = start, End = end }, _transaction, cancellationToken: cancellationToken));
    }
}
