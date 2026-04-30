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
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ISecurityAuditWriter _auditWriter;
    private readonly ILogger<GoogleLoginHandler> _logger;

    public GoogleLoginHandler(
        IUnitOfWork uow,
        UserManager<User> userManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        ISecurityAuditWriter auditWriter,
        ILogger<GoogleLoginHandler> logger)
    {
        _uow                 = uow;
        _userManager         = userManager;
        _tokenService        = tokenService;
        _refreshTokenService = refreshTokenService;
        _auditWriter         = auditWriter;
        _logger              = logger;
    }

    public async Task<Result<TokenResponseDto>> Handle(
        GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await ResolveUserAsync(request, cancellationToken);

        if (user is null)
            return Result.Failure<TokenResponseDto>(ErrorCodes.USER_CREATION_FAILED,
                "Failed to create user from Google account");

        await EnsureEmailConfirmedAsync(user);
        await LinkGoogleLoginIfMissingAsync(user, request.GoogleId);
        await EnsurePersonLoadedAsync(user, cancellationToken);
        await UpdateProfilePictureIfMissingAsync(user, request.PictureUrl);

        var roles = await EnsureRolesAssignedAsync(user);

        var tokenContext = await BuildTokenContextAsync(user, roles, cancellationToken);
        if (tokenContext.IsFailure)
            return Result.Failure<TokenResponseDto>(tokenContext.ErrorCode!, tokenContext.ErrorMessage!);

        var tokens = await IssueTokensAsync(user, roles, tokenContext.Value!, cancellationToken);

        await _auditWriter.WriteAsync(
            user.Id, user.Person.FullName, user.UserName, user.Email,
            string.Join(",", roles), tokenContext.Value!.ClinicId,
            "GoogleLoginSuccess", cancellationToken: cancellationToken);

        _logger.LogInformation("Google OAuth login successful for {Email}", request.Email);

        return Result.Success(tokens);
    }

    // ── Step 1: Find existing user or create a new one ────────────────────────

    private async Task<User?> ResolveUserAsync(GoogleLoginCommand request, CancellationToken ct)
    {
        // Prefer lookup by Google ID — fastest and most reliable
        if (!string.IsNullOrEmpty(request.GoogleId))
        {
            var byLogin = await _userManager.FindByLoginAsync("Google", request.GoogleId);
            if (byLogin is not null) return byLogin;
        }

        // Fall back to email — handles existing password accounts logging in via Google for the first time
        var byEmail = await _uow.Users.GetByEmailOrUsernameAsync(request.Email, ct);
        if (byEmail is not null) return byEmail;

        // Brand new user — create account
        return await CreateUserFromGoogleAsync(request, ct);
    }

    // ── Step 2: Google already verified the email — mark it confirmed ─────────

    private async Task EnsureEmailConfirmedAsync(User user)
    {
        if (user.EmailConfirmed) return;

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
    }

    // ── Step 3: Record the Google login in AspNetUserLogins (standard Identity) ─

    private async Task LinkGoogleLoginIfMissingAsync(User user, string? googleId)
    {
        if (string.IsNullOrEmpty(googleId)) return;

        var existingLogins = await _userManager.GetLoginsAsync(user);
        var alreadyLinked  = existingLogins.Any(l => l.LoginProvider == "Google" && l.ProviderKey == googleId);

        if (!alreadyLinked)
            await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleId, "Google"));
    }

    // ── Step 4: FindByLoginAsync doesn't load navigation — reload if needed ───

    private async Task EnsurePersonLoadedAsync(User user, CancellationToken ct)
    {
        if (user.Person is not null) return;

        var reloaded = await _uow.Users.GetByIdWithPersonAsync(user.Id, ct)
            ?? throw new InvalidOperationException($"User {user.Id} not found after reload.");

        // Copy the loaded Person onto the tracked instance
        user.Person = reloaded.Person;
    }

    // ── Step 5: Store/update Google profile picture ───────────────────────────
    // Always sync the Google picture URL so it stays current.
    // We only skip the update if the user has a custom (non-Google) picture uploaded.

    private static Task UpdateProfilePictureIfMissingAsync(User user, string? pictureUrl)
    {
        if (string.IsNullOrWhiteSpace(pictureUrl)) return Task.CompletedTask;

        var current = user.Person.ProfileImageUrl;

        // Update if: no picture yet, OR current picture is already a Google URL
        bool isGooglePicture = !string.IsNullOrWhiteSpace(current)
            && (current.Contains("googleusercontent.com") || current.Contains("lh3.google"));

        if (string.IsNullOrWhiteSpace(current) || isGooglePicture)
            user.Person.ProfileImageUrl = pictureUrl;

        return Task.CompletedTask;
    }

    // ── Step 6: Assign ClinicOwner role to new users (safety net for edge cases) ─

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

    // ── Step 7: Resolve clinic + member context needed for the JWT ────────────

    private async Task<Result<TokenContext>> BuildTokenContextAsync(
        User user, List<string> roles, CancellationToken ct)
    {
        Guid?   clinicId    = null;
        Guid?   memberId    = null;
        string? countryCode = null;

        if (roles.Contains(UserRoles.ClinicOwner))
        {
            var clinic  = await _uow.Clinics.GetByOwnerIdAsync(user.Id, ct);
            clinicId    = clinic?.Id;
            countryCode = clinic?.CountryCode;
        }
        else
        {
            var member = await _uow.Members.GetByUserIdIgnoreFiltersAsync(user.Id, ct);

            if (member is { IsActive: false })
                return Result.Failure<TokenContext>(ErrorCodes.STAFF_INACTIVE,
                    "Your account has been deactivated.");

            clinicId = member?.ClinicId;
            memberId = member?.Id;

            if (clinicId.HasValue)
            {
                var clinic  = await _uow.Clinics.GetByIdAsync(clinicId.Value, ct);
                countryCode = clinic?.CountryCode;
            }
        }

        // ClinicOwner who is also registered as a doctor — resolve their memberId too
        if (memberId is null && clinicId.HasValue)
        {
            var member = await _uow.Members.GetByUserIdIgnoreFiltersAsync(user.Id, ct);
            memberId   = member?.Id;
        }

        return Result.Success(new TokenContext(clinicId, memberId, countryCode));
    }

    // ── Step 8: Generate access + refresh tokens and stamp last login ─────────

    private async Task<TokenResponseDto> IssueTokensAsync(
        User user, List<string> roles, TokenContext ctx, CancellationToken ct)
    {
        var accessToken  = _tokenService.GenerateAccessToken(user, roles, ctx.MemberId, ctx.ClinicId, ctx.CountryCode);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, ct);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(ct);

        return new TokenResponseDto(accessToken, refreshToken.Token);
    }

    // ── Create: brand new user from Google profile ────────────────────────────

    private async Task<User?> CreateUserFromGoogleAsync(GoogleLoginCommand request, CancellationToken ct)
    {
        var person = new Person
        {
            FullName        = request.FullName,
            Gender          = Gender.Male,       // default — user can update in profile
            ProfileImageUrl = request.PictureUrl,
        };

        await _uow.Persons.AddAsync(person, ct);
        await _uow.SaveChangesAsync(ct);

        var user = new User
        {
            Email          = request.Email,
            UserName       = await GenerateUniqueUsernameAsync(request.Email),
            EmailConfirmed = true,
            PersonId       = person.Id,
            Person         = person,
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

    // ── Internal record to carry JWT context between steps ───────────────────

    private record TokenContext(Guid? ClinicId, Guid? MemberId, string? CountryCode);
}
