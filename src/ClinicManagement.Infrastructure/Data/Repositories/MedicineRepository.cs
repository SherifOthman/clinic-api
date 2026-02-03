using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class MedicineRepository : Repository<Medicine>, IMedicineRepository
{
    public MedicineRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Medicine>> GetByClinicIdAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _context.Medicines
            .Where(m => m.ClinicBranchId == clinicBranchId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Medicine?> GetByIdAndClinicIdAsync(Guid id, Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == id && m.ClinicBranchId == clinicBranchId, cancellationToken);
    }

    public async Task<IEnumerable<Medicine>> GetLowStockMedicinesAsync(Guid clinicBranchId, int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Medicines
            .Where(m => m.ClinicBranchId == clinicBranchId && m.TotalStripsInStock <= threshold)
            .ToListAsync(cancellationToken);
    }
}