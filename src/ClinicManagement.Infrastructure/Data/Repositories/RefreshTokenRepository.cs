
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;
public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {

    }

    public async Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default  )
    {
        return await _dbSet
              .Where(rt => rt.UserId == userId)
              .ToListAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task RemoveAllExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        await _dbSet.Where(rt => rt.ExpiresAt > DateTime.UtcNow)
              .ExecuteDeleteAsync(cancellationToken);
    }


}
