using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly DbSet<Notification> _set;

    public NotificationRepository(ApplicationDbContext context)
        => _set = context.Set<Notification>();

    public Task<PaginatedResult<Notification>> GetPagedAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        return _set
            .Where(n => n.UserId == userId && (n.ExpiresAt == null || n.ExpiresAt > now))
            .OrderByDescending(n => n.CreatedAt)
            .ToPagedAsync(pageNumber, pageSize, ct);
    }

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        return _set.CountAsync(
            n => n.UserId == userId &&
                 !n.IsRead          &&
                 (n.ExpiresAt == null || n.ExpiresAt > now),
            ct);
    }

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(n => n.Id == id, ct);

    public void Add(Notification notification)
        => _set.Add(notification);
}
