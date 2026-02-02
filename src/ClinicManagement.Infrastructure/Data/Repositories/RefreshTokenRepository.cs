using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenRepository(ApplicationDbContext context, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider) : base(context)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var currentTime = _dateTimeProvider.UtcNow;
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiryTime > currentTime, 
                cancellationToken);
    }

    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var currentTime = _dateTimeProvider.UtcNow;
        return await _dbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryTime > currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RefreshToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var currentTime = _dateTimeProvider.UtcNow;
        return await _dbSet
            .Where(rt => rt.IsRevoked || rt.ExpiryTime <= currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await GetExpiredTokensAsync(cancellationToken);
        
        if (expiredTokens.Any())
        {
            _dbSet.RemoveRange(expiredTokens);
            // Note: SaveChangesAsync should be called by UnitOfWork, not repository
        }

        return expiredTokens.Count;
    }
}