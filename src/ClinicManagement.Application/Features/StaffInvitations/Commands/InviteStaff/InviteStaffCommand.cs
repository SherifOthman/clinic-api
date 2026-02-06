using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.StaffInvitations.Commands.InviteStaff;

public record InviteStaffCommand(InviteStaffDto Dto) : IRequest<Result<StaffInvitationDto>>;
