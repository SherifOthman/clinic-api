using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(
    string? RefreshToken
) : IRequest<LogoutResult>;
