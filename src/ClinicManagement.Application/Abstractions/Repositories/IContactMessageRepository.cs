using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IContactMessageRepository
{
    Task AddAsync(ContactMessage message, CancellationToken ct = default);
    Task<PaginatedResult<ContactMessage>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> CountUnreadAsync(CancellationToken ct = default);
}
