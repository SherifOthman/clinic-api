using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class StaffInvitationRepository : BaseRepository<StaffInvitation>, IStaffInvitationRepository
{
    public StaffInvitationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<StaffInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(si => si.Clinic)
            .Include(si => si.InvitedByUser)
            .FirstOrDefaultAsync(si => si.Token == token, cancellationToken);
    }

    public async Task<StaffInvitation?> GetPendingInvitationByEmailAsync(string email, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                si => si.Email == email 
                && si.ClinicId == clinicId 
                && !si.IsAccepted 
                && si.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    public async Task<IEnumerable<StaffInvitation>> GetPendingInvitationsByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(si => si.InvitedByUser)
            .Where(si => si.ClinicId == clinicId && !si.IsAccepted && si.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(si => si.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
