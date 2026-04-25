using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword
) : IRequest<Result>, IAuditableCommand
{
    public string AuditEvent   => "PasswordChanged";
    public string? AuditDetail => null;
}
