using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ReceptionistRepository : Repository<Receptionist>, IReceptionistRepository
{
    public ReceptionistRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Receptionist?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Clinic)
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Receptionist>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.User)
            .Where(r => r.ClinicId == clinicId)
            .ToListAsync(cancellationToken);
    }
}