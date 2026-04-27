using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetActiveTokenAsync(string token, DateTimeOffset now, CancellationToken ct = default)
    {
        var refreshToken = await DbSet.FirstOrDefaultAsync(rt => rt.Token == token, ct);
        return refreshToken is { IsRevoked: false } && refreshToken.ExpiryTime > now ? refreshToken : null;
    }

    public async Task RevokeAllForUserAsync(Guid userId, string ipAddress, DateTimeOffset now, CancellationToken ct = default)
    {
        await DbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryTime > now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.IsRevoked, true)
                .SetProperty(rt => rt.RevokedAt, now)
                .SetProperty(rt => rt.RevokedByIp, ipAddress),
                ct);
    }

    public async Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken ct = default)
    {
        return await DbSet
            .Where(rt => rt.IsRevoked || rt.ExpiryTime <= now)
            .ExecuteDeleteAsync(ct);
    }
}
