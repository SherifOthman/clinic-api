using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ClinicManagement.Application.Features.Auth.Commands;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponseDto>>
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _userLocks = new();

    private readonly IUnitOfWork _uow;
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IUnitOfWork uow,
        UserManager<Domain.Entities.User> userManager,
        IRefreshTokenService refreshTokenService,
        ITokenService tokenService,
        ILogger<RefreshTokenHandler> logger)
    {
        _uow                 = uow;
        _userManager         = userManager;
        _refreshTokenService = refreshTokenService;
        _tokenService        = tokenService;
        _logger              = logger;
    }

    public async Task<Result<TokenResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenEntity = await _refreshTokenService.GetActiveRefreshTokenAsync(request.Token, cancellationToken);
        if (tokenEntity is null)
        {
            _logger.LogWarning("Invalid or expired refresh token");
            return Result.Failure<TokenResponseDto>(ErrorCodes.TOKEN_INVALID, "Invalid or expired refresh token");
        }

        var userId   = tokenEntity.UserId;
        var semaphore = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", userId);
                return Result.Failure<TokenResponseDto>(ErrorCodes.USER_NOT_FOUND, "User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            Guid? clinicId = null;
            string? countryCode = null;
            if (roles.Contains(Roles.ClinicOwner))
            {
                var clinic  = await _uow.Clinics.GetByOwnerIdAsync(user.Id, cancellationToken);
                clinicId    = clinic?.Id;
                countryCode = clinic?.CountryCode;
            }
            else
            {
                var staff = await _uow.Staff.GetByUserIdIgnoreFiltersAsync(user.Id, cancellationToken);
                clinicId  = staff?.ClinicId;

                if (clinicId.HasValue)
                {
                    var clinic  = await _uow.Clinics.GetByIdAsync(clinicId.Value, cancellationToken);
                    countryCode = clinic?.CountryCode;
                }
            }

            var newAccessToken  = _tokenService.GenerateAccessToken(user, roles.ToList(), clinicId, countryCode);
            var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(userId, null, cancellationToken);

            await _refreshTokenService.RevokeRefreshTokenAsync(request.Token, null, newRefreshToken.Token, cancellationToken);

            _logger.LogInformation("Token refreshed for user {UserId} ({ClientType})",
                userId, request.IsMobile ? "mobile" : "web");

            return Result.Success(new TokenResponseDto(newAccessToken, newRefreshToken.Token));
        }
        finally
        {
            semaphore.Release();
            if (semaphore.CurrentCount == 1)
                _userLocks.TryRemove(userId, out _);
        }
    }
}
