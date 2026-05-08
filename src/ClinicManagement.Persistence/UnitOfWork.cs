using ClinicManagement.Application.Abstractions.Data;

namespace ClinicManagement.Persistence;

/// <summary>
/// Owns the transaction boundary. All repositories share the same scoped
/// ApplicationDbContext, so SaveChangesAsync commits everything atomically.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
