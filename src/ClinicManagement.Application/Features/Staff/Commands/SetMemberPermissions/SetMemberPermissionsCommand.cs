using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record SetMemberPermissionsCommand(Guid MemberId, List<Permission> Permissions)
    : IRequest<Result>, IAuditableCommand
{
    public string AuditEvent   => "PermissionsChanged";
    public string? AuditDetail => $"MemberId: {MemberId} | Permissions: [{string.Join(", ", Permissions)}]";
}
