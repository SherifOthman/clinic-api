using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<int> GetCountForClinicByYearAsync(Guid clinicId, int year, CancellationToken cancellationToken = default);
    Task<PagedResult<Invoice>> GetPagedByClinicAsync(Guid clinicId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default);
}
