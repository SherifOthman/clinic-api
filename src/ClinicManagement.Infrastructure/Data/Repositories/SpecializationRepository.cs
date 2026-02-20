using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class SpecializationRepository : ISpecializationRepository
{
    private readonly string _connectionString;

    public SpecializationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<IEnumerable<Specialization>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT Id, NameEn, NameAr, DescriptionEn, DescriptionAr, IsActive, CreatedAt, UpdatedAt, IsDeleted
            FROM Specializations
            WHERE IsDeleted = 0 AND IsActive = 1
            ORDER BY NameEn";

        return await connection.QueryAsync<Specialization>(sql);
    }

    public async Task<Specialization?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT Id, NameEn, NameAr, DescriptionEn, DescriptionAr, IsActive, CreatedAt, UpdatedAt, IsDeleted
            FROM Specializations
            WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";

        return await connection.QueryFirstOrDefaultAsync<Specialization>(sql, new { Id = id });
    }
}
