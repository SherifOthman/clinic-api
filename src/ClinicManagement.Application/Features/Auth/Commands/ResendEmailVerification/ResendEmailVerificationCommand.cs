using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public record ResendEmailVerificationCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
}