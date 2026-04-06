using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands;

public class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISecurityAuditWriter _auditWriter;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        IRefreshTokenService refreshTokenService,
        ICurrentUserService currentUserService,
        ISecurityAuditWriter auditWriter,
        ILogger<LogoutHandler> logger)
    {
        _refreshTokenService = refreshTokenService;
        _currentUserService = currentUserService;
        _auditWriter = auditWriter;
        _logger = logger;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken))
            await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, null, null, cancellationToken);

        await _auditWriter.WriteAsync(
            _currentUserService.UserId,
            _currentUserService.FullName,
            _currentUserService.Username,
            _currentUserService.UserEmail,
            _currentUserService.Roles.FirstOrDefault(),
            _currentUserService.ClinicId,
            "Logout",
            cancellationToken: cancellationToken);

        _logger.LogInformation("User {UserId} logged out", _currentUserService.UserId);
        return Result.Success();
    }
}
