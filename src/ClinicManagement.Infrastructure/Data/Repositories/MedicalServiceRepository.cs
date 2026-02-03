using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class MedicalServiceRepository : Repository<MedicalService>, IMedicalServiceRepository
{
    public MedicalServiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MedicalService>> GetByClinicIdAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalServices
            .Where(s => s.ClinicBranchId == clinicBranchId)
            .ToListAsync(cancellationToken);
    }

    public async Task<MedicalService?> GetByIdAndClinicIdAsync(Guid id, Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalServices
            .FirstOrDefaultAsync(s => s.Id == id && s.ClinicBranchId == clinicBranchId, cancellationToken);
    }
}