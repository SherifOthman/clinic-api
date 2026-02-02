using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class RateLimitRepository : BaseRepository<RateLimitEntry>, IRateLimitRepository
{
    public RateLimitRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RateLimitEntry?> GetActiveEntryAsync(string identifier, string type, DateTime windowStart, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.Identifier == identifier && r.Type == type && r.WindowStart > windowStart, cancellationToken);
    }

    public async Task<int> DeleteExpiredEntriesAsync(DateTime expiredBefore, CancellationToken cancellationToken = default)
    {
        var expiredEntries = await _dbSet
            .Where(r => r.ExpiresAt < expiredBefore)
            .ToListAsync(cancellationToken);

        if (expiredEntries.Any())
        {
            _dbSet.RemoveRange(expiredEntries);
            return expiredEntries.Count;
        }

        return 0;
    }
}