using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(Guid clinicId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Invoice)
            .Where(p => p.Invoice.ClinicId == clinicId && 
                       p.PaymentDate >= startDate && 
                       p.PaymentDate <= endDate)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }
}