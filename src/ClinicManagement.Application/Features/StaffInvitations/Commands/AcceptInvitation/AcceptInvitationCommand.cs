using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.StaffInvitations.Commands.AcceptInvitation;

public record AcceptInvitationCommand(AcceptInvitationDto Dto) : IRequest<Result>;
