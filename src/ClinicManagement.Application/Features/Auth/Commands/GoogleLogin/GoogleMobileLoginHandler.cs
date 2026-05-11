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

/// <summary>
/// Handles Google Sign-In from mobile apps.
///
/// Flow:
///   1. Mobile app uses Google Sign-In SDK → gets an id_token
///   2. App sends id_token to POST /api/auth/oauth/google/mobile
///   3. IGoogleTokenVerifier verifies the token with Google's tokeninfo endpoint
///   4. Extract email, name, sub (googleId), picture
///   5. Delegate to the same find-or-create logic as the web OAuth flow
///
/// Reuses ITokenIssuer and IOAuthUserFactory — no duplication with GoogleLoginHandler.
/// </summary>
public class GoogleMobileLoginHandler : IRequestHandler<GoogleMobileLoginCommand, Result<TokenResponseDto>>
{
    private readonly IUserRepository       _users;
    private readonly IUnitOfWork           _uow;
    private readonly UserManager<User>     _userManager;
    private readonly ITokenIssuer          _tokenIssuer;
    private readonly IOAuthUserFactory     _oAuthUserFactory;
    private readonly IGoogleTokenVerifier  _googleVerifier;
    private readonly IAuditWriter          _audit;
    private readonly ILogger<GoogleMobileLoginHandler> _logger;

    public GoogleMobileLoginHandler(
        IUserRepository users,
        IUnitOfWork uow,
        UserManager<User> userManager,
        ITokenIssuer tokenIssuer,
        IOAuthUserFactory oAuthUserFactory,
        IGoogleTokenVerifier googleVerifier,
        IAuditWriter audit,
        ILogger<GoogleMobileLoginHandler> logger)
    {
        _users            = users;
        _uow              = uow;
        _userManager      = userManager;
        _tokenIssuer      = tokenIssuer;
        _oAuthUserFactory = oAuthUserFactory;
        _googleVerifier   = googleVerifier;
        _audit            = audit;
        _logger           = logger;
    }

    public async Task<Result<TokenResponseDto>> Handle(
        GoogleMobileLoginCommand request, CancellationToken ct)
    {
        // Step 1: Verify the id_token with Google
        var profile = await _googleVerifier.VerifyAsync(request.IdToken, ct);
        if (profile is null)
        {
            _logger.LogWarning("Google mobile login: invalid or expired id_token");
            return Result.Failure<TokenResponseDto>(ErrorCodes.TOKEN_INVALID, "Invalid or expired Google token");
        }

        // Step 2: Find or create the user — same logic as web OAuth
        var user = await ResolveUserAsync(profile, ct);
        if (user is null)
            return Result.Failure<TokenResponseDto>(ErrorCodes.AUTH_METHOD_MISMATCH,
                "This email is registered with a password. Please sign in with your email and password.");

        // Step 3: Ensure email confirmed, link Google login, update picture
        await EnsureEmailConfirmedAsync(user);
        await LinkGoogleLoginIfMissingAsync(user, profile.Sub);
        UpdateProfilePictureIfMissing(user, profile.Picture);

        // Step 4: Issue tokens
        var roles = await EnsureRolesAssignedAsync(user);

        var contextResult = await _tokenIssuer.ResolveContextAsync(user.Id, roles, ct);
        if (contextResult.IsFailure)
            return Result.Failure<TokenResponseDto>(contextResult.ErrorCode!, contextResult.ErrorMessage!);

        var tokens = await _tokenIssuer.IssueTokenPairAsync(user, roles, contextResult.Value!, ct);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(ct);

        await _audit.WriteEventAsync("GoogleMobileLoginSuccess",
            overrideUserId:   user.Id,
            overrideFullName: user.FullName,
            overrideEmail:    user.Email,
            overrideRole:     string.Join(",", roles),
            overrideClinicId: contextResult.Value!.ClinicId,
            ct: ct);

        _logger.LogInformation("Google mobile login successful for {Email}", profile.Email);
        return Result.Success(tokens);
    }

    // ── User resolution — mirrors GoogleLoginHandler ──────────────────────────

    private async Task<User?> ResolveUserAsync(GoogleUserProfile profile, CancellationToken ct)
    {
        // Try by Google login first (fastest path for returning users)
        if (!string.IsNullOrEmpty(profile.Sub))
        {
            var byLogin = await _userManager.FindByLoginAsync("Google", profile.Sub);
            if (byLogin is not null) return byLogin;
        }

        // Try by email
        var byEmail = await _users.GetByEmailOrUsernameAsync(profile.Email, ct);
        if (byEmail is not null)
        {
            var hasPassword    = byEmail.PasswordHash is not null;
            var hasGoogleLogin = (await _userManager.GetLoginsAsync(byEmail))
                .Any(l => l.LoginProvider == "Google");

            // Block if the account was created with a password and has no Google link
            if (hasPassword && !hasGoogleLogin)
            {
                _logger.LogWarning(
                    "Google mobile OAuth blocked for {Email}: account exists with password, no Google login linked.",
                    profile.Email);
                return null;
            }

            return byEmail;
        }

        // New user — create from OAuth profile
        return await _oAuthUserFactory.CreateAsync(
            profile.Email, profile.Name ?? profile.Email, profile.Picture, ct);
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
