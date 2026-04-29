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
        // ── Find or create user ───────────────────────────────────────────────
        // Use IUserRepository (not UserManager) so Person navigation is loaded
        var user = await _uow.Users.GetByEmailOrUsernameAsync(request.Email, cancellationToken);

        if (user is null)
        {
            user = await CreateUserFromGoogleAsync(request, cancellationToken);
            if (user is null)
                return Result.Failure<TokenResponseDto>(
                    ErrorCodes.USER_CREATION_FAILED, "Failed to create user from Google account");
        }
        else if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        // ── Reload user with Person if navigation is null (safety net) ────────
        if (user.Person is null)
        {
            user = await _uow.Users.GetByIdWithPersonAsync(user.Id, cancellationToken);
            if (user is null)
                return Result.Failure<TokenResponseDto>(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        // ── Get roles (reload from DB to ensure fresh after creation) ─────────
        var roles = (await _userManager.GetRolesAsync(user)).ToList();

        // New Google users get ClinicOwner — if roles empty, assign it
        if (!roles.Any())
        {
            await _userManager.AddToRoleAsync(user, UserRoles.ClinicOwner);
            roles = [UserRoles.ClinicOwner];
        }

        Guid? clinicId    = null;
        Guid? memberId    = null;
        string? countryCode = null;

        if (roles.Contains(UserRoles.ClinicOwner))
        {
            var clinic  = await _uow.Clinics.GetByOwnerIdAsync(user.Id, cancellationToken);
            clinicId    = clinic?.Id;
            countryCode = clinic?.CountryCode;
        }
        else
        {
            var member = await _uow.Members.GetByUserIdIgnoreFiltersAsync(user.Id, cancellationToken);
            if (member is not null && !member.IsActive)
                return Result.Failure<TokenResponseDto>(
                    ErrorCodes.STAFF_INACTIVE, "Your account has been deactivated.");

            clinicId = member?.ClinicId;
            memberId = member?.Id;

            if (clinicId.HasValue)
            {
                var clinic  = await _uow.Clinics.GetByIdAsync(clinicId.Value, cancellationToken);
                countryCode = clinic?.CountryCode;
            }
        }

        // Resolve memberId for ClinicOwner who is also a doctor
        if (memberId is null && clinicId.HasValue)
        {
            var member = await _uow.Members.GetByUserIdIgnoreFiltersAsync(user.Id, cancellationToken);
            memberId   = member?.Id;
        }

        // ── Generate tokens ───────────────────────────────────────────────────
        var accessToken  = _tokenService.GenerateAccessToken(
            user, roles.ToList(), memberId, clinicId, countryCode);

        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(
            user.Id, null, cancellationToken);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            user.Id, user.Person.FullName, user.UserName, user.Email,
            string.Join(",", roles), clinicId, "GoogleLoginSuccess",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Google OAuth login successful for {Email}", request.Email);

        return Result.Success(new TokenResponseDto(accessToken, refreshToken.Token));
    }

    // ── Private: create user from Google profile ──────────────────────────────

    private async Task<User?> CreateUserFromGoogleAsync(
        GoogleLoginCommand request, CancellationToken ct)
    {
        var person = new Person
        {
            FullName = request.FullName,
            Gender   = Gender.Male, // default — user can update in profile
        };

        await _uow.Persons.AddAsync(person, ct);
        await _uow.SaveChangesAsync(ct);

        // Generate a unique username from the email prefix
        var baseUsername = request.Email.Split('@')[0];
        var username     = await EnsureUniqueUsernameAsync(baseUsername);

        var user = new User
        {
            Email          = request.Email,
            UserName       = username,
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

    private async Task<string> EnsureUniqueUsernameAsync(string baseUsername)
    {
        var candidate = baseUsername;
        var suffix    = 1;

        while (await _userManager.FindByNameAsync(candidate) is not null)
            candidate = $"{baseUsername}{suffix++}";

        return candidate;
    }
}
