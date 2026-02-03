using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
}
