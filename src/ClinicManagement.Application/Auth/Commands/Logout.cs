using ClinicManagement.Application.Abstractions.Authentication;`nusing ClinicManagement.Application.Abstractions.Email;`nusing ClinicManagement.Application.Abstractions.Services;`nusing ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands;

public record LogoutCommand(
    string? RefreshToken
) : IRequest<Result>;

public class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        IRefreshTokenService refreshTokenService,
        ILogger<LogoutHandler> logger)
    {
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            await _refreshTokenService.RevokeRefreshTokenAsync(
                request.RefreshToken,
                null,
                null,
                cancellationToken);

            _logger.LogInformation("User logged out and refresh token revoked");
        }

        return Result.Success();
    }
}
