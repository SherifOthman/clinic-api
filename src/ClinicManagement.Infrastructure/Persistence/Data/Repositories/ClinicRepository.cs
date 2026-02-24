using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class ClinicRepository : IClinicRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public ClinicRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<Clinic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, OwnerUserId, SubscriptionPlanId, OnboardingCompleted, IsActive, 
                   CreatedAt, IsDeleted, SubscriptionStartDate, SubscriptionEndDate, 
                   TrialEndDate, BillingEmail
            FROM Clinics 
            WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<Clinic>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Clinic?> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, OwnerUserId, SubscriptionPlanId, OnboardingCompleted, IsActive, 
                   CreatedAt, IsDeleted, SubscriptionStartDate, SubscriptionEndDate, 
                   TrialEndDate, BillingEmail
            FROM Clinics 
            WHERE OwnerUserId = @OwnerUserId";
        return await _connection.QueryFirstOrDefaultAsync<Clinic>(
            new CommandDefinition(sql, new { OwnerUserId = ownerUserId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Clinic>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, OwnerUserId, SubscriptionPlanId, OnboardingCompleted, IsActive, 
                   CreatedAt, IsDeleted, SubscriptionStartDate, SubscriptionEndDate, 
                   TrialEndDate, BillingEmail
            FROM Clinics";
        return await _connection.QueryAsync<Clinic>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Clinic> AddAsync(Clinic entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Clinics (
                Id, Name, OwnerUserId, SubscriptionPlanId, OnboardingCompleted, IsActive, 
                CreatedAt, IsDeleted, SubscriptionStartDate, SubscriptionEndDate, 
                TrialEndDate, BillingEmail
            ) VALUES (
                @Id, @Name, @OwnerUserId, @SubscriptionPlanId, @OnboardingCompleted, @IsActive, 
                GETUTCDATE(), 0, @SubscriptionStartDate, @SubscriptionEndDate, 
                @TrialEndDate, @BillingEmail
            )";
        
        await _connection.ExecuteAsync(
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
                IsActive = @IsActive,
                SubscriptionStartDate = @SubscriptionStartDate,
                SubscriptionEndDate = @SubscriptionEndDate,
                TrialEndDate = @TrialEndDate,
                BillingEmail = @BillingEmail
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Clinics WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
