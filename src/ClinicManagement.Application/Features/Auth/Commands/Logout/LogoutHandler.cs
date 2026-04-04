using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands;

public class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        IApplicationDbContext context,
        IRefreshTokenService refreshTokenService,
        ICurrentUserService currentUserService,
        ILogger<LogoutHandler> logger)
    {
        _context = context;
        _refreshTokenService = refreshTokenService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            await _refreshTokenService.RevokeRefreshTokenAsync(
                request.RefreshToken, null, null, cancellationToken);
        }

        _context.AuditLogs.Add(new AuditLog
        {
            Timestamp  = DateTime.UtcNow,
            ClinicId   = _currentUserService.ClinicId,
            UserId     = _currentUserService.UserId,
            UserName   = _currentUserService.FullName,
            UserRole   = _currentUserService.Roles.FirstOrDefault(),
            EntityType = "Auth",
            EntityId   = _currentUserService.UserId?.ToString() ?? "unknown",
            Action     = AuditAction.Security,
            IpAddress  = _currentUserService.IpAddress,
            Changes    = "{\"event\":\"Logout\"}",
        });
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged out", _currentUserService.UserId);
        return Result.Success();
    }
}
