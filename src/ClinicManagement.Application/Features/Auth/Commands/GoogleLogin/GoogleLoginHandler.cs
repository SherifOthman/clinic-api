using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, Result<TokenResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;
    private readonly ITokenIssuer _tokenIssuer;
    private readonly IAuditWriter _audit;
    private readonly ILogger<GoogleLoginHandler> _logger;

    public GoogleLoginHandler(
        IUnitOfWork uow,
        UserManager<User> userManager,
        ITokenIssuer tokenIssuer,
        IAuditWriter audit,
        ILogger<GoogleLoginHandler> logger)
    {
        _uow         = uow;
        _userManager = userManager;
        _tokenIssuer = tokenIssuer;
        _audit       = audit;
        _logger      = logger;
    }

    public async Task<Result<TokenResponseDto>> Handle(
        GoogleLoginCommand request, CancellationToken ct)
    {
        var user = await ResolveUserAsync(request, ct);
        if (user is null)
            return Result.Failure<TokenResponseDto>(ErrorCodes.USER_CREATION_FAILED,
                "Failed to create user from Google account");

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

    // ── Step 1: Find existing user or create a new one ────────────────────────

    private async Task<User?> ResolveUserAsync(GoogleLoginCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.GoogleId))
        {
            var byLogin = await _userManager.FindByLoginAsync("Google", request.GoogleId);
            if (byLogin is not null) return byLogin;
        }

        var byEmail = await _uow.Users.GetByEmailOrUsernameAsync(request.Email, ct);
        if (byEmail is not null) return byEmail;

        return await CreateUserFromGoogleAsync(request, ct);
    }

    // ── Step 2: Google already verified the email ─────────────────────────────

    private async Task EnsureEmailConfirmedAsync(User user)
    {
        if (user.EmailConfirmed) return;
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
    }

    // ── Step 3: Record the Google login in AspNetUserLogins ──────────────────

    private async Task LinkGoogleLoginIfMissingAsync(User user, string? googleId)
    {
        if (string.IsNullOrEmpty(googleId)) return;
        var existingLogins = await _userManager.GetLoginsAsync(user);
        if (!existingLogins.Any(l => l.LoginProvider == "Google" && l.ProviderKey == googleId))
            await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleId, "Google"));
    }

    // ── Step 4: Sync Google profile picture ──────────────────────────────────

    private static void UpdateProfilePictureIfMissing(User user, string? pictureUrl)
    {
        if (string.IsNullOrWhiteSpace(pictureUrl)) return;

        var current         = user.ProfileImageUrl;
        var isGooglePicture = !string.IsNullOrWhiteSpace(current)
            && (current.Contains("googleusercontent.com") || current.Contains("lh3.google"));

        if (string.IsNullOrWhiteSpace(current) || isGooglePicture)
            user.ProfileImageUrl = pictureUrl;
    }

    // ── Step 5: Assign ClinicOwner role to new users ──────────────────────────

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

    // ── Create: brand new user from Google profile ────────────────────────────

    private async Task<User?> CreateUserFromGoogleAsync(GoogleLoginCommand request, CancellationToken ct)
    {
        var user = new User
        {
            Email           = request.Email,
            UserName        = await GenerateUniqueUsernameAsync(request.Email),
            EmailConfirmed  = true,
            FullName        = request.FullName,
            Gender          = Gender.Male,
            ProfileImageUrl = request.PictureUrl,
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to create user from Google OAuth {Email}: {Errors}",
                request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        _logger.LogInformation("Created new user from Google OAuth: {Email}", request.Email);
        return user;
    }

    private async Task<string> GenerateUniqueUsernameAsync(string email)
    {
        var baseUsername = email.Split('@')[0];
        var candidate    = baseUsername;
        var suffix       = 1;

        while (await _userManager.FindByNameAsync(candidate) is not null)
            candidate = $"{baseUsername}{suffix++}";

        return candidate;
    }
}
