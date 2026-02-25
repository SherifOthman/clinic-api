using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Auth.Commands.Login;

public record LoginCommand(
    string EmailOrUsername,
    string Password,
    bool IsMobile
) : IRequest<Result<LoginResponseDto>>;

public record LoginResponseDto(
    string AccessToken,
    string? RefreshToken
);
