using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands;

public class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAuditWriter _audit;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        IRefreshTokenService refreshTokenService,
        IAuditWriter audit,
        ILogger<LogoutHandler> logger)
    {
        _refreshTokenService = refreshTokenService;
        _audit               = audit;
        _logger              = logger;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken))
            await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, null, null, cancellationToken);

        // Current user context is read automatically by IAuditWriter
        await _audit.WriteEventAsync("Logout", ct: cancellationToken);

        _logger.LogInformation("User logged out");
        return Result.Success();
    }
}
