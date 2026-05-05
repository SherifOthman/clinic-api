using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(
    int PageNumber = 1,
    int PageSize   = 20
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<NotificationDto>>>;

public record NotificationDto(
    Guid            Id,
    string          Type,
    string          Title,
    string          Message,
    string?         ActionUrl,
    bool            IsRead,
    DateTimeOffset  CreatedAt
);
