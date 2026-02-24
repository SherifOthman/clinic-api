using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class StaffInvitationRepository : IStaffInvitationRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public StaffInvitationRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<StaffInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<StaffInvitation>> GetPendingByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM StaffInvitation 
            WHERE ClinicId = @ClinicId 
            AND IsAccepted = 0 
            AND IsCanceled = 0
            AND ExpiresAt > GETUTCDATE()
            AND IsDeleted = 0
            ORDER BY CreatedAt DESC";
        
        return await _connection.QueryAsync<StaffInvitation>(
            new CommandDefinition(sql, new { ClinicId = clinicId }, _transaction, cancellationToken: cancellationToken));
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
                Id, ClinicId, Email, Role, SpecializationId, InvitationToken, ExpiresAt, 
                IsAccepted, AcceptedAt, AcceptedByUserId, CreatedByUserId, IsDeleted
            ) VALUES (
                @Id, @ClinicId, @Email, @Role, @SpecializationId, @InvitationToken, @ExpiresAt,
                @IsAccepted, @AcceptedAt, @AcceptedByUserId, @CreatedByUserId, @IsDeleted
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(StaffInvitation entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE StaffInvitation SET
                Email = @Email,
                Role = @Role,
                SpecializationId = @SpecializationId,
                InvitationToken = @InvitationToken,
                ExpiresAt = @ExpiresAt,
                IsAccepted = @IsAccepted,
                IsCanceled = @IsCanceled,
                AcceptedAt = @AcceptedAt,
                AcceptedByUserId = @AcceptedByUserId,
                CreatedByUserId = @CreatedByUserId
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE StaffInvitation SET IsDeleted = 1 WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
