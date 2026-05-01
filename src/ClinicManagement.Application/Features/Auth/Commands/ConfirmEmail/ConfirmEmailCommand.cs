using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    Guid UserId,
    string Token
) : IRequest<Result>;
