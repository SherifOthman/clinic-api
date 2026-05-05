using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Notifications.Commands.MarkRead;

public class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, Result>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationReadHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var userId = _currentUser.GetRequiredUserId();
        var now    = DateTimeOffset.UtcNow;

        if (request.NotificationId is null)
        {
            // Mark all unread notifications for this user
            var paged = await _uow.Notifications.GetPagedAsync(userId, 1, 200, ct);
            foreach (var n in paged.Items.Where(n => !n.IsRead))
            {
                n.IsRead = true;
                n.ReadAt = now;
            }
        }
        else
        {
            var notification = await _uow.Notifications.GetByIdAsync(request.NotificationId.Value, ct);

            if (notification is null)
                return Result.Failure(ErrorCodes.NOT_FOUND, "Notification not found");

            if (notification.UserId != userId)
                return Result.Failure(ErrorCodes.FORBIDDEN, "Access denied");

            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
