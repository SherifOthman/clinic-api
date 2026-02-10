using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenRepository(ApplicationDbContext context, IDateTimeProvider dateTimeProvider) : base(context)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<RefreshToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        return await _dbSet
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiryTime > now, cancellationToken);
    }

    public async Task<List<RefreshToken>> GetActiveTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        return await _dbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryTime > now)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RefreshToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        return await _dbSet
            .Where(rt => rt.IsRevoked || rt.ExpiryTime <= now)
            .ToListAsync(cancellationToken);
    }
}
