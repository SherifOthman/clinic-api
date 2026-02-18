using ClinicManagement.Application.Abstractions.Authentication;`nusing ClinicManagement.Application.Abstractions.Email;`nusing ClinicManagement.Application.Abstractions.Services;`nusing ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ClinicManagement.Application.Auth.Commands;

public record RefreshTokenCommand(
    string Token,
    bool IsMobile
) : IRequest<Result<RefreshTokenResponseDto>>;

public record RefreshTokenResponseDto(
    string AccessToken,
    string? RefreshToken
);

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponseDto>>
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _userLocks = new();

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IUnitOfWork unitOfWork,
        IRefreshTokenService refreshTokenService,
        ITokenService tokenService,
        ILogger<RefreshTokenHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _refreshTokenService = refreshTokenService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<RefreshTokenResponseDto>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Validate refresh token exists and is active
        var tokenEntity = await _refreshTokenService.GetActiveRefreshTokenAsync(request.Token, cancellationToken);
        if (tokenEntity == null)
        {
            _logger.LogWarning("Invalid or expired refresh token");
            return Result.Failure<RefreshTokenResponseDto>(ErrorCodes.TOKEN_INVALID, "Invalid or expired refresh token");
        }

        var userId = tokenEntity.UserId;

        // Use per-user semaphore to prevent concurrent refresh requests
        var semaphore = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            // Get user and roles
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", userId);
                return Result.Failure<RefreshTokenResponseDto>(ErrorCodes.USER_NOT_FOUND, "User not found");
            }

            var roles = await _unitOfWork.Users.GetUserRolesAsync(userId, cancellationToken);
            var staff = await _unitOfWork.Users.GetStaffByUserIdAsync(userId, cancellationToken);
            var clinicId = staff?.ClinicId;

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles, clinicId);
            var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(userId, null, cancellationToken);

            // Revoke old refresh token
            await _refreshTokenService.RevokeRefreshTokenAsync(request.Token, null, newRefreshToken.Token, cancellationToken);

            _logger.LogInformation("Token refreshed for user {UserId} ({ClientType})",
                userId, request.IsMobile ? "mobile" : "web");

            var response = new RefreshTokenResponseDto(newAccessToken, newRefreshToken.Token);
            return Result.Success(response);
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
