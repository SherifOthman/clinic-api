using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands;

public record RefreshTokenCommand(
    string Token,
    bool IsMobile
) : IRequest<Result<RefreshTokenResponseDto>>;

public record RefreshTokenResponseDto(
    string AccessToken,
    string? RefreshToken
);
