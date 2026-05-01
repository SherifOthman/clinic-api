using ClinicManagement.Application.Features.Staff.Dtos;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record InviteStaffCommand(string Role, string Email, Guid? SpecializationId = null)
    : IRequest<Result<InviteStaffResponseDto>>;
