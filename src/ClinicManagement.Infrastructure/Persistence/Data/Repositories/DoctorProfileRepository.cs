using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class DoctorProfileRepository : IDoctorProfileRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public DoctorProfileRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<DoctorProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, StaffId, SpecializationId, YearsOfExperience, 
                   LicenseNumber, LicenseExpiryDate, Bio, CreatedAt, UpdatedAt
            FROM DoctorProfiles 
            WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<DoctorProfile>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<DoctorProfile?> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, StaffId, SpecializationId, YearsOfExperience, 
                   LicenseNumber, LicenseExpiryDate, Bio, CreatedAt, UpdatedAt
            FROM DoctorProfiles 
            WHERE StaffId = @StaffId";
        return await _connection.QueryFirstOrDefaultAsync<DoctorProfile>(
            new CommandDefinition(sql, new { StaffId = staffId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<DoctorProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, StaffId, SpecializationId, YearsOfExperience, 
                   LicenseNumber, LicenseExpiryDate, Bio, CreatedAt, UpdatedAt
            FROM DoctorProfiles";
        return await _connection.QueryAsync<DoctorProfile>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<DoctorProfile> AddAsync(DoctorProfile entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO DoctorProfiles (
                Id, StaffId, SpecializationId, YearsOfExperience,
                LicenseNumber, LicenseExpiryDate, Bio, CreatedAt
            ) VALUES (
                @Id, @StaffId, @SpecializationId, @YearsOfExperience,
                @LicenseNumber, @LicenseExpiryDate, @Bio, GETUTCDATE()
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(DoctorProfile entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE DoctorProfiles SET
                StaffId = @StaffId,
                SpecializationId = @SpecializationId,
                YearsOfExperience = @YearsOfExperience,
                LicenseNumber = @LicenseNumber,
                LicenseExpiryDate = @LicenseExpiryDate,
                Bio = @Bio,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM DoctorProfiles WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
