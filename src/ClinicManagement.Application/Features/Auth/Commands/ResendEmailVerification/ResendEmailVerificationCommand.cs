using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public record ResendEmailVerificationCommand(
    string Email
) : IRequest<ResendEmailVerificationResult>;
