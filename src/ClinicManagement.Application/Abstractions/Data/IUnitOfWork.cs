namespace ClinicManagement.Application.Abstractions.Data;

/// <summary>
/// Owns the transaction boundary. Call SaveChangesAsync() to commit all staged
/// changes atomically. Repositories are injected directly into handlers — not
/// accessed through this interface — so each handler only depends on what it uses.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
