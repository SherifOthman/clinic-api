using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class ContactMessageRepository : IContactMessageRepository
{
    private readonly DbSet<ContactMessage> _set;

    public ContactMessageRepository(ApplicationDbContext context)
        => _set = context.Set<ContactMessage>();

    public async Task AddAsync(ContactMessage message, CancellationToken ct = default)
        => await _set.AddAsync(message, ct);

    public Task<PaginatedResult<ContactMessage>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        => _set.OrderByDescending(m => m.CreatedAt)
               .ToPagedAsync(pageNumber, pageSize, ct);

    public Task<int> CountUnreadAsync(CancellationToken ct = default)
        => _set.CountAsync(m => !m.IsRead, ct);

    public Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(m => m.Id == id, ct);
}
