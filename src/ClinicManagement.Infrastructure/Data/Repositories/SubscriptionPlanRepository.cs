using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public SubscriptionPlanRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<SubscriptionPlan?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM SubscriptionPlans WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<SubscriptionPlan>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM SubscriptionPlans ORDER BY MonthlyFee";
        return await _connection.QueryAsync<SubscriptionPlan>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM SubscriptionPlans WHERE IsActive = 1 ORDER BY MonthlyFee";
        return await _connection.QueryAsync<SubscriptionPlan>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<SubscriptionPlan> AddAsync(SubscriptionPlan entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO SubscriptionPlans (
                Name, NameAr, Description, DescriptionAr, MonthlyFee, YearlyFee,
                SetupFee, MaxBranches, MaxStaff, MaxPatientsPerMonth, MaxAppointmentsPerMonth,
                MaxInvoicesPerMonth, StorageLimitGB, HasInventoryManagement, HasReporting,
                IsActive, IsPopular, DisplayOrder
            ) VALUES (
                @Name, @NameAr, @Description, @DescriptionAr, @MonthlyFee, @YearlyFee,
                @SetupFee, @MaxBranches, @MaxStaff, @MaxPatientsPerMonth, @MaxAppointmentsPerMonth,
                @MaxInvoicesPerMonth, @StorageLimitGB, @HasInventoryManagement, @HasReporting,
                @IsActive, @IsPopular, @DisplayOrder
            );
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        entity.Id = await _connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(SubscriptionPlan entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE SubscriptionPlans SET
                Name = @Name,
                NameAr = @NameAr,
                Description = @Description,
                DescriptionAr = @DescriptionAr,
                MonthlyFee = @MonthlyFee,
                YearlyFee = @YearlyFee,
                SetupFee = @SetupFee,
                MaxBranches = @MaxBranches,
                MaxStaff = @MaxStaff,
                MaxPatientsPerMonth = @MaxPatientsPerMonth,
                MaxAppointmentsPerMonth = @MaxAppointmentsPerMonth,
                MaxInvoicesPerMonth = @MaxInvoicesPerMonth,
                StorageLimitGB = @StorageLimitGB,
                HasInventoryManagement = @HasInventoryManagement,
                HasReporting = @HasReporting,
                IsActive = @IsActive,
                IsPopular = @IsPopular,
                DisplayOrder = @DisplayOrder
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM SubscriptionPlans WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
