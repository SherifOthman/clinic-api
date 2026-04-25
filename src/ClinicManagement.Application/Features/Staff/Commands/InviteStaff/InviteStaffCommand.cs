using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Dtos;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record InviteStaffCommand(string Role, string Email, Guid? SpecializationId = null)
    : IRequest<Result<InviteStaffResponseDto>>, IAuditableCommand
{
    public string AuditEvent   => "StaffInvited";
    public string? AuditDetail => $"Invited {Email} as {Role}";
}
