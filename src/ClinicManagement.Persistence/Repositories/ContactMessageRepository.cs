using ClinicManagement.Application.Abstractions.Repositories;
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

    public Task<List<ContactMessage>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        => _set.OrderByDescending(m => m.CreatedAt)
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync(ct);

    public Task<int> CountUnreadAsync(CancellationToken ct = default)
        => _set.CountAsync(m => !m.IsRead, ct);
}
