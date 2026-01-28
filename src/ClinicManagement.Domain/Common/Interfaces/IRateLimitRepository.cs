using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IRateLimitRepository : IRepository<RateLimitEntry>
{
    Task<RateLimitEntry?> GetActiveEntryAsync(string identifier, string type, DateTime windowStart, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredEntriesAsync(DateTime expiredBefore, CancellationToken cancellationToken = default);
}