using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record ResendInvitationCommand(Guid InvitationId) : IRequest<Result>;
