using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class StaffBranchRepository : IStaffBranchRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public StaffBranchRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<StaffBranch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM StaffBranches WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<StaffBranch>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<StaffBranch>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM StaffBranches ORDER BY IsPrimaryBranch DESC, StartDate DESC";
        return await _connection.QueryAsync<StaffBranch>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<StaffBranch> AddAsync(StaffBranch entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO StaffBranches (
                Id, StaffId, ClinicBranchId, IsPrimaryBranch, 
                StartDate, EndDate, IsActive, CreatedAt, UpdatedAt
            ) VALUES (
                @Id, @StaffId, @ClinicBranchId, @IsPrimaryBranch, 
                @StartDate, @EndDate, @IsActive, @CreatedAt, @UpdatedAt
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(StaffBranch entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE StaffBranches SET
                StaffId = @StaffId,
                ClinicBranchId = @ClinicBranchId,
                IsPrimaryBranch = @IsPrimaryBranch,
                StartDate = @StartDate,
                EndDate = @EndDate,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM StaffBranches WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<StaffBranch>> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM StaffBranches 
            WHERE StaffId = @StaffId 
            ORDER BY IsPrimaryBranch DESC, StartDate DESC";
        
        return await _connection.QueryAsync<StaffBranch>(
            new CommandDefinition(sql, new { StaffId = staffId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<StaffBranch>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM StaffBranches 
            WHERE ClinicBranchId = @BranchId AND IsActive = 1 
            ORDER BY IsPrimaryBranch DESC, StartDate DESC";
        
        return await _connection.QueryAsync<StaffBranch>(
            new CommandDefinition(sql, new { BranchId = branchId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<StaffBranch?> GetPrimaryBranchAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM StaffBranches 
            WHERE StaffId = @StaffId AND IsPrimaryBranch = 1 AND IsActive = 1";
        
        return await _connection.QueryFirstOrDefaultAsync<StaffBranch>(
            new CommandDefinition(sql, new { StaffId = staffId }, _transaction, cancellationToken: cancellationToken));
    }
}
