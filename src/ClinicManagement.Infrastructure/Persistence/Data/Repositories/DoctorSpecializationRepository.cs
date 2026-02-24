using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class DoctorSpecializationRepository : IDoctorSpecializationRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public DoctorSpecializationRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<DoctorSpecialization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM DoctorSpecializations WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<DoctorSpecialization>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<DoctorSpecialization>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM DoctorSpecializations ORDER BY IsPrimary DESC, CreatedAt";
        return await _connection.QueryAsync<DoctorSpecialization>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<DoctorSpecialization> AddAsync(DoctorSpecialization entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO DoctorSpecializations (
                Id, DoctorProfileId, SpecializationId, IsPrimary, 
                YearsOfExperience, CertificationNumber, CertificationDate, CreatedAt
            ) VALUES (
                @Id, @DoctorProfileId, @SpecializationId, @IsPrimary, 
                @YearsOfExperience, @CertificationNumber, @CertificationDate, @CreatedAt
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(DoctorSpecialization entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE DoctorSpecializations SET
                DoctorProfileId = @DoctorProfileId,
                SpecializationId = @SpecializationId,
                IsPrimary = @IsPrimary,
                YearsOfExperience = @YearsOfExperience,
                CertificationNumber = @CertificationNumber,
                CertificationDate = @CertificationDate
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM DoctorSpecializations WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<DoctorSpecialization>> GetByDoctorIdAsync(Guid doctorProfileId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM DoctorSpecializations 
            WHERE DoctorProfileId = @DoctorProfileId 
            ORDER BY IsPrimary DESC, YearsOfExperience DESC";
        
        return await _connection.QueryAsync<DoctorSpecialization>(
            new CommandDefinition(sql, new { DoctorProfileId = doctorProfileId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task UpdatePrimaryAsync(Guid doctorProfileId, Guid specializationId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE DoctorSpecializations 
            SET IsPrimary = CASE WHEN SpecializationId = @SpecializationId THEN 1 ELSE 0 END
            WHERE DoctorProfileId = @DoctorProfileId";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { DoctorProfileId = doctorProfileId, SpecializationId = specializationId }, _transaction, cancellationToken: cancellationToken));
    }
}
