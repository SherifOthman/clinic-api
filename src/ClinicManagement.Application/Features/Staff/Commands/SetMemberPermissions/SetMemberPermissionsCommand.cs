using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

/// <summary>
/// Accepts raw permission strings from the API — parsing happens in the handler,
/// keeping the controller thin and the validation centralized.
/// </summary>
public record SetMemberPermissionsCommand(Guid MemberId, List<string> RawPermissions)
    : IRequest<Result>, IAuditableCommand
{
    public string AuditEvent   => "PermissionsChanged";
    public string? AuditDetail => $"MemberId: {MemberId}";
}
