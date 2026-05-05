using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Notifications.Queries.GetUnreadCount;

public record GetUnreadCountQuery : IRequest<Result<int>>;
