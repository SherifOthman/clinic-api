using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class MeasurementAttributeRepository : BaseRepository<MeasurementAttribute>, IMeasurementAttributeRepository
{
    public MeasurementAttributeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<MeasurementAttribute?> GetByNameAsync(string nameEn, string? nameAr = null, CancellationToken cancellationToken = default)
    {
        var query = _context.MeasurementAttributes.AsQueryable();
        
        if (!string.IsNullOrEmpty(nameAr))
        {
            return await query.FirstOrDefaultAsync(m => 
                m.NameEn.ToLower() == nameEn.ToLower() || 
                m.NameAr.ToLower() == nameAr.ToLower(), 
                cancellationToken);
        }
        
        return await query.FirstOrDefaultAsync(m => 
            m.NameEn.ToLower() == nameEn.ToLower(), 
            cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string nameEn, string? nameAr = null, CancellationToken cancellationToken = default)
    {
        var query = _context.MeasurementAttributes.AsQueryable();
        
        if (!string.IsNullOrEmpty(nameAr))
        {
            return await query.AnyAsync(m => 
                m.NameEn.ToLower() == nameEn.ToLower() || 
                m.NameAr.ToLower() == nameAr.ToLower(), 
                cancellationToken);
        }
        
        return await query.AnyAsync(m => 
            m.NameEn.ToLower() == nameEn.ToLower(), 
            cancellationToken);
    }
}