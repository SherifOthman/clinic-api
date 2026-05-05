using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsHandler
    : IRequestHandler<GetNotificationsQuery, Result<PaginatedResult<NotificationDto>>>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationsHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedResult<NotificationDto>>> Handle(
        GetNotificationsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.GetRequiredUserId();

        var paged = await _uow.Notifications.GetPagedAsync(
            userId, request.PageNumber, request.PageSize, ct);

        var dtos = paged.Items
            .Select(n => new NotificationDto(
                n.Id,
                n.Type.ToString(),
                n.Title,
                n.Message,
                n.ActionUrl,
                n.IsRead,
                n.CreatedAt))
            .ToList();

        return Result.Success(
            PaginatedResult<NotificationDto>.Create(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize));
    }
}
