using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicRepository : IClinicRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public ClinicRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<Clinic?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Clinics WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<Clinic>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Clinic?> GetByOwnerUserIdAsync(int ownerUserId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Clinics WHERE OwnerUserId = @OwnerUserId";
        return await _connection.QueryFirstOrDefaultAsync<Clinic>(
            new CommandDefinition(sql, new { OwnerUserId = ownerUserId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Clinic>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Clinics";
        return await _connection.QueryAsync<Clinic>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Clinic> AddAsync(Clinic entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Clinics (Name, OwnerUserId, SubscriptionPlanId, OnboardingCompleted, IsActive, CreatedAt, IsDeleted)
            OUTPUT INSERTED.Id
            VALUES (@Name, @OwnerUserId, @SubscriptionPlanId, @OnboardingCompleted, @IsActive, GETUTCDATE(), 0)";
        
        entity.Id = await _connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(Clinic entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Clinics 
            SET Name = @Name, 
                SubscriptionPlanId = @SubscriptionPlanId, 
                OnboardingCompleted = @OnboardingCompleted, 
                IsActive = @IsActive
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Clinics WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}

