using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface INotificationRepository
{
    /// <summary>Returns paginated notifications for a user, newest first.</summary>
    Task<PaginatedResult<Notification>> GetPagedAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>Count of unread, non-expired notifications for a user.</summary>
    Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default);

    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);

    void Add(Notification notification);
}
