using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string Token,
    bool IsMobile
) : IRequest<RefreshTokenResult>;
