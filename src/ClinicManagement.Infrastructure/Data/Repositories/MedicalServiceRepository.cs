using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class MedicalServiceRepository : BaseRepository<MedicalService>, IMedicalServiceRepository
{
    public MedicalServiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MedicalService>> GetByClinicBranchIdAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.ClinicBranchId == clinicBranchId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<MedicalService?> GetByIdAndClinicBranchIdAsync(Guid id, Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.ClinicBranchId == clinicBranchId, cancellationToken);
    }
}