using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class ClinicSubscriptionRepository : IClinicSubscriptionRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public ClinicSubscriptionRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<ClinicSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ClinicSubscriptions WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<ClinicSubscription>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ClinicSubscription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ClinicSubscriptions ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<ClinicSubscription>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<ClinicSubscription> AddAsync(ClinicSubscription entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO ClinicSubscriptions (
                Id, ClinicId, SubscriptionPlanId, Status, StartDate, EndDate, 
                TrialEndDate, AutoRenew, CancellationReason, CancelledAt, 
                CancelledBy, CreatedAt, UpdatedAt
            ) VALUES (
                @Id, @ClinicId, @SubscriptionPlanId, @Status, @StartDate, @EndDate, 
                @TrialEndDate, @AutoRenew, @CancellationReason, @CancelledAt, 
                @CancelledBy, @CreatedAt, @UpdatedAt
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(ClinicSubscription entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE ClinicSubscriptions SET
                ClinicId = @ClinicId,
                SubscriptionPlanId = @SubscriptionPlanId,
                Status = @Status,
                StartDate = @StartDate,
                EndDate = @EndDate,
                TrialEndDate = @TrialEndDate,
                AutoRenew = @AutoRenew,
                CancellationReason = @CancellationReason,
                CancelledAt = @CancelledAt,
                CancelledBy = @CancelledBy,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM ClinicSubscriptions WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<ClinicSubscription?> GetActiveByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM ClinicSubscriptions 
            WHERE ClinicId = @ClinicId AND Status = @ActiveStatus 
            ORDER BY CreatedAt DESC";
        
        return await _connection.QueryFirstOrDefaultAsync<ClinicSubscription>(
            new CommandDefinition(sql, new { ClinicId = clinicId, ActiveStatus = (int)SubscriptionStatus.Active }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ClinicSubscription>> GetExpiringSubscriptionsAsync(int daysBeforeExpiry, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM ClinicSubscriptions 
            WHERE Status = @ActiveStatus 
            AND EndDate IS NOT NULL 
            AND EndDate <= DATEADD(day, @DaysBeforeExpiry, GETUTCDATE())
            AND EndDate > GETUTCDATE()
            ORDER BY EndDate";
        
        return await _connection.QueryAsync<ClinicSubscription>(
            new CommandDefinition(sql, new { DaysBeforeExpiry = daysBeforeExpiry, ActiveStatus = (int)SubscriptionStatus.Active }, _transaction, cancellationToken: cancellationToken));
    }
}
