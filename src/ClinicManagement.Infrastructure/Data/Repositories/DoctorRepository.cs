using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class DoctorRepository : Repository<Doctor>, IDoctorRepository
{
    public DoctorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Doctor>> GetBySpecializationAsync(int specializationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.SpecializationId == specializationId && d.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Doctor>> GetActiveDoctorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Doctor>> GetByBranchIdAsync(int branchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.DoctorBranches.Any(db => db.BranchId == branchId) && d.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Doctor?> GetWithSpecializationAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Specialization)
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);
    }
}

