using ClinicManagement.Domain.Common;

namespace ClinicManagement.Application.Abstractions.Data;

/// <summary>
/// ORM-agnostic repository contract.
///
/// Only primitive parameters — no lambdas, no expressions, no LINQ.
/// Any ORM (EF Core, Dapper, ADO.NET) can implement this without knowing
/// about expression trees or IQueryable.
///
/// Complex queries with filtering belong in child repository interfaces
/// as named methods with explicit primitive parameters.
/// </summary>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    // ── Reads ─────────────────────────────────────────────────────────────────

    /// <summary>Find by primary key. Returns null if not found.</summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Total count of all entities (respects active query filters).</summary>
    Task<int> CountAsync(CancellationToken ct = default);

    // ── Writes ────────────────────────────────────────────────────────────────

    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}
