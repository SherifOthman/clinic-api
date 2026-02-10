using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<decimal> GetTotalPaidByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
