using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class MedicineRepository : Repository<Medicine>, IMedicineRepository
{
    public MedicineRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Medicine>> SearchMedicinesAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(m =>
                m.Name.Contains(searchTerm) ||
                (m.GenericName != null && m.GenericName.Contains(searchTerm)));
        }

        return await query.ToListAsync(cancellationToken);
    }
}
