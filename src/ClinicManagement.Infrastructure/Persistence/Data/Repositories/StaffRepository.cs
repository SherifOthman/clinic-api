using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public StaffRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, ClinicId, IsActive, HireDate, CreatedAt, IsDeleted,
                   IsPrimaryClinic, Status, StatusChangedAt, StatusChangedBy, StatusReason, TerminationDate
            FROM Staff 
            WHERE Id = @Id AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<Staff>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Staff>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, ClinicId, IsActive, HireDate, CreatedAt, IsDeleted,
                   IsPrimaryClinic, Status, StatusChangedAt, StatusChangedBy, StatusReason, TerminationDate
            FROM Staff 
            WHERE ClinicId = @ClinicId AND IsDeleted = 0 
            ORDER BY HireDate DESC";
        return await _connection.QueryAsync<Staff>(
            new CommandDefinition(sql, new { ClinicId = clinicId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Staff>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, ClinicId, IsActive, HireDate, CreatedAt, IsDeleted,
                   IsPrimaryClinic, Status, StatusChangedAt, StatusChangedBy, StatusReason, TerminationDate
            FROM Staff 
            WHERE IsDeleted = 0";
        return await _connection.QueryAsync<Staff>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Staff> AddAsync(Staff entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Staff (
                Id, UserId, ClinicId, IsActive, HireDate, IsPrimaryClinic, Status
            ) VALUES (
                @Id, @UserId, @ClinicId, @IsActive, @HireDate, @IsPrimaryClinic, @Status
            )";
        
        await _connection.ExecuteAsync(
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
                HireDate = @HireDate,
                IsPrimaryClinic = @IsPrimaryClinic,
                Status = @Status,
                StatusChangedAt = @StatusChangedAt,
                StatusChangedBy = @StatusChangedBy,
                StatusReason = @StatusReason,
                TerminationDate = @TerminationDate
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE Staff SET IsDeleted = 1 WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid clinicId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT CAST(CASE WHEN EXISTS(
                SELECT 1 FROM Staff 
                WHERE UserId = @UserId AND ClinicId = @ClinicId AND IsDeleted = 0
            ) THEN 1 ELSE 0 END AS BIT)";
        
        return await _connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { UserId = userId, ClinicId = clinicId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task UpdateStatusAsync(Guid staffId, string status, Guid changedBy, string reason, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Staff SET
                Status = @Status,
                StatusChangedAt = GETUTCDATE(),
                StatusChangedBy = @ChangedBy,
                StatusReason = @Reason,
                TerminationDate = CASE WHEN @Status IN ('Terminated', 'Resigned') THEN GETUTCDATE() ELSE TerminationDate END
            WHERE Id = @StaffId";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { StaffId = staffId, Status = status, ChangedBy = changedBy, Reason = reason }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Staff>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, ClinicId, IsActive, HireDate, CreatedAt, IsDeleted,
                   IsPrimaryClinic, Status, StatusChangedAt, StatusChangedBy, StatusReason, TerminationDate
            FROM Staff 
            WHERE UserId = @UserId AND IsDeleted = 0 
            ORDER BY IsPrimaryClinic DESC, HireDate DESC";
        
        return await _connection.QueryAsync<Staff>(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }
}
