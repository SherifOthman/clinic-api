using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicBranchAppointmentPriceRepository : BaseRepository<ClinicBranchAppointmentPrice>, IClinicBranchAppointmentPriceRepository
{
    public ClinicBranchAppointmentPriceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ClinicBranchAppointmentPrice>> GetByClinicBranchAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicBranchAppointmentPrices
            .Include(cbap => cbap.AppointmentType)
            .Where(cbap => cbap.ClinicBranchId == clinicBranchId)
            .OrderBy(cbap => cbap.AppointmentType.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ClinicBranchAppointmentPrice>> GetActiveByClinicBranchAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicBranchAppointmentPrices
            .Include(cbap => cbap.AppointmentType)
            .Where(cbap => cbap.ClinicBranchId == clinicBranchId && cbap.IsActive && cbap.AppointmentType.IsActive)
            .OrderBy(cbap => cbap.AppointmentType.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClinicBranchAppointmentPrice?> GetByClinicBranchAndTypeAsync(Guid clinicBranchId, Guid appointmentTypeId, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicBranchAppointmentPrices
            .Include(cbap => cbap.AppointmentType)
            .Include(cbap => cbap.ClinicBranch)
            .FirstOrDefaultAsync(cbap => cbap.ClinicBranchId == clinicBranchId && cbap.AppointmentTypeId == appointmentTypeId, cancellationToken);
    }

    public async Task<decimal?> GetPriceAsync(Guid clinicBranchId, Guid appointmentTypeId, CancellationToken cancellationToken = default)
    {
        var pricing = await _context.ClinicBranchAppointmentPrices
            .Where(cbap => cbap.ClinicBranchId == clinicBranchId && 
                          cbap.AppointmentTypeId == appointmentTypeId && 
                          cbap.IsActive)
            .Select(cbap => cbap.Price)
            .FirstOrDefaultAsync(cancellationToken);

        return pricing == 0 ? null : pricing;
    }
}
