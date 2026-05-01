using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword
) : IRequest<Result>;
