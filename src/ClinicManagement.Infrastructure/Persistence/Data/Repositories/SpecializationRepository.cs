using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class SpecializationRepository : ISpecializationRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public SpecializationRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<IEnumerable<Specialization>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, NameEn, NameAr, DescriptionEn, DescriptionAr, IsActive, CreatedAt, UpdatedAt, IsDeleted
            FROM Specializations
            WHERE IsDeleted = 0 AND IsActive = 1
            ORDER BY NameEn";

        return await _connection.QueryAsync<Specialization>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Specialization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, NameEn, NameAr, DescriptionEn, DescriptionAr, IsActive, CreatedAt, UpdatedAt, IsDeleted
            FROM Specializations
            WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";

        return await _connection.QueryFirstOrDefaultAsync<Specialization>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
