using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ClinicManagement.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _userLocks = new();
    
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IRefreshTokenService refreshTokenService,
        IAuthenticationService authService,
        ILogger<RefreshTokenHandler> logger)
    {
        _refreshTokenService = refreshTokenService;
        _authService = authService;
        _logger = logger;
    }

    public async Task<RefreshTokenResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Validate refresh token exists and is active
        var tokenEntity = await _refreshTokenService.GetActiveRefreshTokenAsync(request.Token, cancellationToken);
        if (tokenEntity == null)
        {
            _logger.LogWarning("Invalid or expired refresh token");
            return new RefreshTokenResult(
                Success: false,
                AccessToken: null,
                RefreshToken: null,
                ErrorCode: ErrorCodes.TOKEN_INVALID,
                ErrorMessage: "Invalid or expired refresh token"
            );
        }

        var userId = tokenEntity.UserId;
        
        // Use per-user semaphore to prevent concurrent refresh requests
        var semaphore = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var result = await _authService.RefreshTokenAsync(request.Token, cancellationToken);
            
            if (result == null)
            {
                _logger.LogWarning("Token refresh failed for user {UserId}", userId);
                return new RefreshTokenResult(
                    Success: false,
                    AccessToken: null,
                    RefreshToken: null,
                    ErrorCode: ErrorCodes.TOKEN_INVALID,
                    ErrorMessage: "Failed to refresh token"
                );
            }

            _logger.LogInformation("Token refreshed for user {UserId} ({ClientType})", 
                userId, request.IsMobile ? "mobile" : "web");

            return new RefreshTokenResult(
                Success: true,
                AccessToken: result.AccessToken,
                RefreshToken: result.RefreshToken,
                ErrorCode: null,
                ErrorMessage: null
            );
        }
        finally
        {
            semaphore.Release();
            
            // Clean up semaphore if no one is waiting
            if (semaphore.CurrentCount == 1)
            {
                _userLocks.TryRemove(userId, out _);
            }
        }
    }
}
