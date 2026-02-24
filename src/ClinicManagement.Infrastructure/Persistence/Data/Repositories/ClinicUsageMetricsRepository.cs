using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class ClinicUsageMetricsRepository : IClinicUsageMetricsRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public ClinicUsageMetricsRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<ClinicUsageMetrics?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ClinicUsageMetrics WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<ClinicUsageMetrics>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ClinicUsageMetrics>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ClinicUsageMetrics ORDER BY MetricDate DESC";
        return await _connection.QueryAsync<ClinicUsageMetrics>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<ClinicUsageMetrics> AddAsync(ClinicUsageMetrics entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO ClinicUsageMetrics (
                Id, ClinicId, MetricDate, ActiveStaffCount, NewPatientsCount, 
                TotalPatientsCount, AppointmentsCount, InvoicesCount, 
                StorageUsedGB, CreatedAt, UpdatedAt
            ) VALUES (
                @Id, @ClinicId, @MetricDate, @ActiveStaffCount, @NewPatientsCount, 
                @TotalPatientsCount, @AppointmentsCount, @InvoicesCount, 
                @StorageUsedGB, @CreatedAt, @UpdatedAt
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(ClinicUsageMetrics entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE ClinicUsageMetrics SET
                ClinicId = @ClinicId,
                MetricDate = @MetricDate,
                ActiveStaffCount = @ActiveStaffCount,
                NewPatientsCount = @NewPatientsCount,
                TotalPatientsCount = @TotalPatientsCount,
                AppointmentsCount = @AppointmentsCount,
                InvoicesCount = @InvoicesCount,
                StorageUsedGB = @StorageUsedGB,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM ClinicUsageMetrics WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<ClinicUsageMetrics?> GetByClinicAndDateAsync(Guid clinicId, DateTime date, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM ClinicUsageMetrics 
            WHERE ClinicId = @ClinicId AND MetricDate = @MetricDate";
        
        return await _connection.QueryFirstOrDefaultAsync<ClinicUsageMetrics>(
            new CommandDefinition(sql, new { ClinicId = clinicId, MetricDate = date.Date }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ClinicUsageMetrics>> GetMonthlyUsageAsync(Guid clinicId, DateTime monthStart, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM ClinicUsageMetrics 
            WHERE ClinicId = @ClinicId 
            AND MetricDate >= @MonthStart 
            AND MetricDate < DATEADD(month, 1, @MonthStart)
            ORDER BY MetricDate";
        
        return await _connection.QueryAsync<ClinicUsageMetrics>(
            new CommandDefinition(sql, new { ClinicId = clinicId, MonthStart = monthStart.Date }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task UpsertAsync(ClinicUsageMetrics metrics, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            MERGE ClinicUsageMetrics AS target
            USING (SELECT @ClinicId AS ClinicId, @MetricDate AS MetricDate) AS source
            ON target.ClinicId = source.ClinicId AND target.MetricDate = source.MetricDate
            WHEN MATCHED THEN
                UPDATE SET
                    ActiveStaffCount = @ActiveStaffCount,
                    NewPatientsCount = @NewPatientsCount,
                    TotalPatientsCount = @TotalPatientsCount,
                    AppointmentsCount = @AppointmentsCount,
                    InvoicesCount = @InvoicesCount,
                    StorageUsedGB = @StorageUsedGB,
                    UpdatedAt = @UpdatedAt
            WHEN NOT MATCHED THEN
                INSERT (Id, ClinicId, MetricDate, ActiveStaffCount, NewPatientsCount, 
                        TotalPatientsCount, AppointmentsCount, InvoicesCount, 
                        StorageUsedGB, CreatedAt, UpdatedAt)
                VALUES (@Id, @ClinicId, @MetricDate, @ActiveStaffCount, @NewPatientsCount, 
                        @TotalPatientsCount, @AppointmentsCount, @InvoicesCount, 
                        @StorageUsedGB, @CreatedAt, @UpdatedAt);";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, metrics, _transaction, cancellationToken: cancellationToken));
    }
}
