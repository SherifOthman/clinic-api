using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Auth.Commands;

public record RefreshTokenCommand(
    string Token,
    bool IsMobile
) : IRequest<Result<RefreshTokenResponseDto>>;

public record RefreshTokenResponseDto(
    string AccessToken,
    string? RefreshToken
);
