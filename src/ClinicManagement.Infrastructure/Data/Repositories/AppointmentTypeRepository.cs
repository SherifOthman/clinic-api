using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class AppointmentTypeRepository : BaseRepository<AppointmentType>, IAppointmentTypeRepository
{
    public AppointmentTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AppointmentType>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AppointmentTypes
            .Where(at => at.IsActive)
            .OrderBy(at => at.DisplayOrder)
            .ThenBy(at => at.NameEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<AppointmentType?> GetByNameAsync(string nameEn, CancellationToken cancellationToken = default)
    {
        return await _context.AppointmentTypes
            .FirstOrDefaultAsync(at => at.NameEn == nameEn, cancellationToken);
    }

    public async Task<IEnumerable<AppointmentType>> GetOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AppointmentTypes
            .OrderBy(at => at.DisplayOrder)
            .ThenBy(at => at.NameEn)
            .ToListAsync(cancellationToken);
    }
}