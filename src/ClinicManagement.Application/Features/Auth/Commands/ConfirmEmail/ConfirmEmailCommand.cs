using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    string Email,
    string Token
) : IRequest<ConfirmEmailResult>;
