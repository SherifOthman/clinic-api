using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record SetStaffActiveStatusCommand(Guid StaffId, bool IsActive)
    : IRequest<Result>, IAuditableCommand
{
    public string AuditEvent   => IsActive ? "StaffActivated" : "StaffDeactivated";
    public string? AuditDetail => $"StaffId: {StaffId}";
}
