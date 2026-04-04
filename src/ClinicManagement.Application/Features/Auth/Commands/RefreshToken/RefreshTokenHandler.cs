using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ClinicManagement.Application.Features.Auth.Commands;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponseDto>>
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _userLocks = new();

    private readonly IApplicationDbContext _context;
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IApplicationDbContext context,
        UserManager<Domain.Entities.User> userManager,
        IRefreshTokenService refreshTokenService,
        ITokenService tokenService,
        ILogger<RefreshTokenHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _refreshTokenService = refreshTokenService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<RefreshTokenResponseDto>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var tokenEntity = await _refreshTokenService.GetActiveRefreshTokenAsync(request.Token, cancellationToken);
        if (tokenEntity == null)
        {
            _logger.LogWarning("Invalid or expired refresh token");
            return Result.Failure<RefreshTokenResponseDto>(ErrorCodes.TOKEN_INVALID, "Invalid or expired refresh token");
        }

        var userId = tokenEntity.UserId;
        var semaphore = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", userId);
                return Result.Failure<RefreshTokenResponseDto>(ErrorCodes.USER_NOT_FOUND, "User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            Guid? clinicId = null;
            if (roles.Contains(Roles.ClinicOwner))
            {
                var clinic = await _context.Clinics
                    .IgnoreQueryFilters([QueryFilterNames.Tenant])
                    .FirstOrDefaultAsync(c => c.OwnerUserId == user.Id, cancellationToken);
                clinicId = clinic?.Id;
            }
            else
            {
                var staff = await _context.Staff
                    .IgnoreQueryFilters([QueryFilterNames.Tenant])
                    .FirstOrDefaultAsync(s => s.UserId == user.Id, cancellationToken);
                clinicId = staff?.ClinicId;
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user, roles.ToList(), clinicId);
            var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(userId, null, cancellationToken);

            await _refreshTokenService.RevokeRefreshTokenAsync(request.Token, null, newRefreshToken.Token, cancellationToken);

            _logger.LogInformation("Token refreshed for user {UserId} ({ClientType})",
                userId, request.IsMobile ? "mobile" : "web");

            var response = new RefreshTokenResponseDto(newAccessToken, newRefreshToken.Token);
            return Result.Success(response);
        }
        finally
        {
            semaphore.Release();

            if (semaphore.CurrentCount == 1)
            {
                _userLocks.TryRemove(userId, out _);
            }
        }
    }
}
