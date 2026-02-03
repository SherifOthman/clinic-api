using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Invoice>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.ClinicId == clinicId)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && i.ClinicId == clinicId, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByPatientIdAsync(Guid clinicPatientId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.ClinicPatientId == clinicPatientId)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.ClinicId == clinicId && i.RemainingAmount > 0)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetWithItemsAndPaymentsAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items)
                .ThenInclude(item => item.MedicalService)
            .Include(i => i.Items)
                .ThenInclude(item => item.Medicine)
            .Include(i => i.Items)
                .ThenInclude(item => item.MedicalSupply)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && i.ClinicId == clinicId, cancellationToken);
    }
}