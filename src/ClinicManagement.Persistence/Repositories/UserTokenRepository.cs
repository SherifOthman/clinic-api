using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class UserTokenRepository : IUserTokenRepository
{
    private readonly DbSet<UserToken> _tokens;

    public UserTokenRepository(ApplicationDbContext context)
        => _tokens = context.Set<UserToken>();

    public async Task AddAsync(UserToken token, CancellationToken ct = default)
        => await _tokens.AddAsync(token, ct);

    public async Task<UserToken?> GetActiveByHashAsync(
        string tokenHash, DateTimeOffset now, CancellationToken ct = default)
        => await _tokens
            .Where(t => t.TokenHash == tokenHash && !t.IsUsed && t.ExpiresAt > now)
            .FirstOrDefaultAsync(ct);

    public async Task InvalidateAllAsync(
        Guid userId, string tokenType, CancellationToken ct = default)
        => await _tokens
            .Where(t => t.UserId == userId && t.TokenType == tokenType && !t.IsUsed)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsUsed, true)
                .SetProperty(t => t.UsedAt, DateTimeOffset.UtcNow),
                ct);

    public async Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken ct = default)
        => await _tokens
            .Where(t => t.IsUsed || t.ExpiresAt <= now)
            .ExecuteDeleteAsync(ct);
}
