using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class MedicalVisitRepository : IMedicalVisitRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public MedicalVisitRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<MedicalVisit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, ClinicBranchId, PatientId, DoctorId, AppointmentId, 
                   Diagnosis, CreatedAt, UpdatedAt
            FROM MedicalVisits 
            WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<MedicalVisit>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<MedicalVisit>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, ClinicBranchId, PatientId, DoctorId, AppointmentId, 
                   Diagnosis, CreatedAt, UpdatedAt
            FROM MedicalVisits 
            ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<MedicalVisit>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<MedicalVisit>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, ClinicBranchId, PatientId, DoctorId, AppointmentId, 
                   Diagnosis, CreatedAt, UpdatedAt
            FROM MedicalVisits 
            WHERE PatientId = @PatientId 
            ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<MedicalVisit>(
            new CommandDefinition(sql, new { PatientId = patientId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<MedicalVisit>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, ClinicBranchId, PatientId, DoctorId, AppointmentId, 
                   Diagnosis, CreatedAt, UpdatedAt
            FROM MedicalVisits 
            WHERE DoctorId = @DoctorId 
            ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<MedicalVisit>(
            new CommandDefinition(sql, new { DoctorId = doctorId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<MedicalVisit?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, ClinicBranchId, PatientId, DoctorId, AppointmentId, 
                   Diagnosis, CreatedAt, UpdatedAt
            FROM MedicalVisits 
            WHERE AppointmentId = @AppointmentId";
        return await _connection.QueryFirstOrDefaultAsync<MedicalVisit>(
            new CommandDefinition(sql, new { AppointmentId = appointmentId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<MedicalVisit> AddAsync(MedicalVisit entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO MedicalVisits (
                Id, ClinicBranchId, PatientId, DoctorId, AppointmentId, 
                Diagnosis, CreatedAt
            ) VALUES (
                @Id, @ClinicBranchId, @PatientId, @DoctorId, @AppointmentId, 
                @Diagnosis, GETUTCDATE()
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(MedicalVisit entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE MedicalVisits SET
                ClinicBranchId = @ClinicBranchId,
                PatientId = @PatientId,
                DoctorId = @DoctorId,
                AppointmentId = @AppointmentId,
                Diagnosis = @Diagnosis,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM MedicalVisits WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
