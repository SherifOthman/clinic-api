using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<IEnumerable<Invoice>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByPatientIdAsync(Guid clinicPatientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetWithItemsAndPaymentsAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default);
}