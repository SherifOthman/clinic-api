using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Notifications.Commands.MarkRead;

/// <summary>Marks a single notification as read. Pass Guid.Empty to mark all.</summary>
public record MarkNotificationReadCommand(Guid? NotificationId) : IRequest<Result>;
