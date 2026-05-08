using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, Result<TokenResponseDto>>
{
    private readonly IUserRepository    _users;
    private readonly IUnitOfWork        _uow;
    private readonly UserManager<User>  _userManager;
    private readonly ITokenIssuer       _tokenIssuer;
    private readonly IOAuthUserFactory  _oAuthUserFactory;
    private readonly IAuditWriter       _audit;
    private readonly ILogger<GoogleLoginHandler> _logger;

    public GoogleLoginHandler(
        IUserRepository users,
        IUnitOfWork uow,
        UserManager<User> userManager,
        ITokenIssuer tokenIssuer,
        IOAuthUserFactory oAuthUserFactory,
        IAuditWriter audit,
        ILogger<GoogleLoginHandler> logger)
    {
        _users            = users;
        _uow              = uow;
        _userManager      = userManager;
        _tokenIssuer      = tokenIssuer;
        _oAuthUserFactory = oAuthUserFactory;
        _audit            = audit;
        _logger           = logger;
    }

    public async Task<Result<TokenResponseDto>> Handle(
        GoogleLoginCommand request, CancellationToken ct)
    {
        var user = await ResolveUserAsync(request, ct);
        if (user is null)
            return Result.Failure<TokenResponseDto>(ErrorCodes.AUTH_METHOD_MISMATCH,
                "This email is registered with a password. Please sign in with your email and password.");

        await EnsureEmailConfirmedAsync(user);
        await LinkGoogleLoginIfMissingAsync(user, request.GoogleId);
        UpdateProfilePictureIfMissing(user, request.PictureUrl);

        var roles = await EnsureRolesAssignedAsync(user);

        var contextResult = await _tokenIssuer.ResolveContextAsync(user.Id, roles, ct);
        if (contextResult.IsFailure)
            return Result.Failure<TokenResponseDto>(contextResult.ErrorCode!, contextResult.ErrorMessage!);

        var tokens = await _tokenIssuer.IssueTokenPairAsync(user, roles, contextResult.Value!, ct);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(ct);

        await _audit.WriteEventAsync("GoogleLoginSuccess",
            overrideUserId:   user.Id,
            overrideFullName: user.FullName,
            overrideEmail:    user.Email,
            overrideRole:     string.Join(",", roles),
            overrideClinicId: contextResult.Value!.ClinicId,
            ct: ct);

        _logger.LogInformation("Google OAuth login successful for {Email}", request.Email);
        return Result.Success(tokens);
    }

    private async Task<User?> ResolveUserAsync(GoogleLoginCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.GoogleId))
        {
            var byLogin = await _userManager.FindByLoginAsync("Google", request.GoogleId);
            if (byLogin is not null) return byLogin;
        }

        var byEmail = await _users.GetByEmailOrUsernameAsync(request.Email, ct);
        if (byEmail is not null)
        {
            var hasPassword    = byEmail.PasswordHash is not null;
            var hasGoogleLogin = (await _userManager.GetLoginsAsync(byEmail))
                .Any(l => l.LoginProvider == "Google");

            if (hasPassword && !hasGoogleLogin)
            {
                _logger.LogWarning(
                    "Google OAuth blocked for {Email}: account exists with password, no Google login linked.",
                    request.Email);
                return null;
            }

            return byEmail;
        }

        return await _oAuthUserFactory.CreateAsync(request.Email, request.FullName, request.PictureUrl, ct);
    }

    private async Task EnsureEmailConfirmedAsync(User user)
    {
        if (user.EmailConfirmed) return;
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
    }

    private async Task LinkGoogleLoginIfMissingAsync(User user, string? googleId)
    {
        if (string.IsNullOrEmpty(googleId)) return;
        var existingLogins = await _userManager.GetLoginsAsync(user);
        if (!existingLogins.Any(l => l.LoginProvider == "Google" && l.ProviderKey == googleId))
            await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleId, "Google"));
    }

    private static void UpdateProfilePictureIfMissing(User user, string? pictureUrl)
    {
        if (string.IsNullOrWhiteSpace(pictureUrl)) return;

        var current         = user.ProfileImageUrl;
        var isGooglePicture = !string.IsNullOrWhiteSpace(current)
            && (current.Contains("googleusercontent.com") || current.Contains("lh3.google"));

        if (string.IsNullOrWhiteSpace(current) || isGooglePicture)
            user.ProfileImageUrl = pictureUrl;
    }

    private async Task<List<string>> EnsureRolesAssignedAsync(User user)
    {
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        if (roles.Count == 0)
        {
            await _userManager.AddToRoleAsync(user, UserRoles.ClinicOwner);
            roles = [UserRoles.ClinicOwner];
        }
        return roles;
    }
}
