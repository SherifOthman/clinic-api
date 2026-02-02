using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class UserClinicRepository : BaseRepository<UserClinic>, IUserClinicRepository
{
    public UserClinicRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<UserClinic>> GetUserClinicsWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uc => uc.Clinic)
            .ThenInclude(c => c.SubscriptionPlan)
            .Where(uc => uc.UserId == userId && uc.IsActive)
            .OrderByDescending(uc => uc.IsOwner)
            .ThenBy(uc => uc.Clinic.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserClinic?> GetUserClinicWithDetailsAsync(Guid userId, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uc => uc.Clinic)
            .ThenInclude(c => c.SubscriptionPlan)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClinicId == clinicId && uc.IsActive, cancellationToken);
    }
}