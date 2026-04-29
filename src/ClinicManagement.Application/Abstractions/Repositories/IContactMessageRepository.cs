using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IContactMessageRepository
{
    Task AddAsync(ContactMessage message, CancellationToken ct = default);
    Task<List<ContactMessage>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<int> CountUnreadAsync(CancellationToken ct = default);
}
