using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Application database context interface.
/// Defines the contract for data access without depending on EF Core implementation.
/// </summary>
public interface IApplicationDbContext
{
    // Reference data
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
