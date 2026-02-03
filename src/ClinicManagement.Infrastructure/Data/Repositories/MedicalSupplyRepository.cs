using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class MedicalSupplyRepository : Repository<MedicalSupply>, IMedicalSupplyRepository
{
    public MedicalSupplyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MedicalSupply>> GetByClinicIdAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalSupplies
            .Where(s => s.ClinicBranchId == clinicBranchId)
            .ToListAsync(cancellationToken);
    }

    public async Task<MedicalSupply?> GetByIdAndClinicIdAsync(Guid id, Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalSupplies
            .FirstOrDefaultAsync(s => s.Id == id && s.ClinicBranchId == clinicBranchId, cancellationToken);
    }

    public async Task<IEnumerable<MedicalSupply>> GetLowStockSuppliesAsync(Guid clinicBranchId, int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalSupplies
            .Where(s => s.ClinicBranchId == clinicBranchId && s.QuantityInStock <= threshold)
            .ToListAsync(cancellationToken);
    }
}