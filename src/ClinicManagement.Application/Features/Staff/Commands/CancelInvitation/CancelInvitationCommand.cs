using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record CancelInvitationCommand(Guid InvitationId) : IRequest<Result>;
