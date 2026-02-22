using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Auth.Commands.ResendEmailVerification;

public record ResendEmailVerificationCommand(
    string Email
) : IRequest<Result>;
