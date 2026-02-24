using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class ClinicBranchRepository : IClinicBranchRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public ClinicBranchRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<ClinicBranch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ClinicBranches WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<ClinicBranch>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ClinicBranch>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ClinicBranches";
        return await _connection.QueryAsync<ClinicBranch>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<ClinicBranch> AddAsync(ClinicBranch entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO ClinicBranches (Id, ClinicId, Name, AddressLine, CountryGeoNameId, StateGeoNameId, CityGeoNameId, IsMainBranch, IsActive, CreatedAt, IsDeleted)
            VALUES (@Id, @ClinicId, @Name, @AddressLine, @CountryGeoNameId, @StateGeoNameId, @CityGeoNameId, @IsMainBranch, @IsActive, GETUTCDATE(), 0)";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(ClinicBranch entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE ClinicBranches 
            SET Name = @Name,
                AddressLine = @AddressLine,
                CountryGeoNameId = @CountryGeoNameId,
                StateGeoNameId = @StateGeoNameId,
                CityGeoNameId = @CityGeoNameId,
                IsMainBranch = @IsMainBranch,
                IsActive = @IsActive
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM ClinicBranches WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
