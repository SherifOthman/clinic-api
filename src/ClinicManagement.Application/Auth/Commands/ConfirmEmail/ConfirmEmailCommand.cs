using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    string Email,
    string Token
) : IRequest<Result>;
