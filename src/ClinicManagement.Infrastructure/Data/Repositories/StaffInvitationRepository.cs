using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class StaffInvitationRepository : IStaffInvitationRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public StaffInvitationRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<StaffInvitation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM StaffInvitation WHERE Id = @Id AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<StaffInvitation>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<StaffInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM StaffInvitation WHERE InvitationToken = @Token AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<StaffInvitation>(
            new CommandDefinition(sql, new { Token = token }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<StaffInvitation>> GetPendingByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM StaffInvitation 
            WHERE ClinicId = @ClinicId 
            AND IsAccepted = 0 
            AND ExpiresAt > GETUTCDATE()
            AND IsDeleted = 0
            ORDER BY CreatedAt DESC";
        
        return await _connection.QueryAsync<StaffInvitation>(
            new CommandDefinition(sql, new { ClinicId = clinicId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> HasPendingInvitationAsync(int clinicId, string role, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT CAST(CASE WHEN EXISTS(
                SELECT 1 FROM StaffInvitation 
                WHERE ClinicId = @ClinicId 
                AND Role = @Role 
                AND IsAccepted = 0 
                AND ExpiresAt > GETUTCDATE()
                AND IsDeleted = 0
            ) THEN 1 ELSE 0 END AS BIT)";
        
        return await _connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { ClinicId = clinicId, Role = role }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<StaffInvitation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM StaffInvitation WHERE IsDeleted = 0";
        return await _connection.QueryAsync<StaffInvitation>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<StaffInvitation> AddAsync(StaffInvitation entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO StaffInvitation (
                ClinicId, Email, Role, InvitationToken, ExpiresAt, 
                IsAccepted, AcceptedAt, AcceptedByUserId, CreatedByUserId, CreatedAt, IsDeleted
            ) VALUES (
                @ClinicId, @Email, @Role, @InvitationToken, @ExpiresAt,
                @IsAccepted, @AcceptedAt, @AcceptedByUserId, @CreatedByUserId, @CreatedAt, @IsDeleted
            );
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        entity.Id = await _connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(StaffInvitation entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE StaffInvitation SET
                Email = @Email,
                Role = @Role,
                InvitationToken = @InvitationToken,
                ExpiresAt = @ExpiresAt,
                IsAccepted = @IsAccepted,
                AcceptedAt = @AcceptedAt,
                AcceptedByUserId = @AcceptedByUserId,
                CreatedByUserId = @CreatedByUserId
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE StaffInvitation SET IsDeleted = 1 WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
