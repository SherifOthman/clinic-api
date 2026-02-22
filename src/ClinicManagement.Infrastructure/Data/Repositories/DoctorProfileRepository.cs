using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class DoctorProfileRepository : IDoctorProfileRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public DoctorProfileRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<DoctorProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM DoctorProfiles WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<DoctorProfile>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<DoctorProfile?> GetByStaffIdAsync(int staffId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM DoctorProfiles WHERE StaffId = @StaffId";
        return await _connection.QueryFirstOrDefaultAsync<DoctorProfile>(
            new CommandDefinition(sql, new { StaffId = staffId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<DoctorProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM DoctorProfiles";
        return await _connection.QueryAsync<DoctorProfile>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<DoctorProfile> AddAsync(DoctorProfile entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO DoctorProfiles (
                StaffId, SpecializationId, YearsOfExperience
            ) VALUES (
                @StaffId, @SpecializationId, @YearsOfExperience
            );
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        entity.Id = await _connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(DoctorProfile entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE DoctorProfiles SET
                StaffId = @StaffId,
                SpecializationId = @SpecializationId,
                YearsOfExperience = @YearsOfExperience
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM DoctorProfiles WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
