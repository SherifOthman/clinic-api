using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IOptions<JwtOptions> jwtOptions, 
        IRefreshTokenService refreshTokenService,
        IDateTimeProvider dateTimeProvider,
        ILogger<TokenService> logger)
    {
        _jwtOptions = jwtOptions.Value;
        _refreshTokenService = refreshTokenService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles, Guid? clinicId = null)
    {
        // Use CurrentClinicId if available, otherwise fall back to ClinicId for backward compatibility
        var effectiveClinicId = clinicId ?? user.CurrentClinicId ?? user.ClinicId;
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimConstants.ClinicId, effectiveClinicId?.ToString() ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);
        return refreshToken.Token;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken, null, null, cancellationToken);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var (principal, _) = ValidateAccessTokenWithExpiry(token);
        return principal;
    }

    public bool IsTokenExpired(string token)
    {
        var (_, isExpired) = ValidateAccessTokenWithExpiry(token);
        return isExpired;
    }

    public (ClaimsPrincipal? principal, bool isExpired) ValidateAccessTokenWithExpiry(string token)
    {
        if (string.IsNullOrEmpty(token))
            return (null, false);

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = securityKey,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return (principal, false);
        }
        catch (SecurityTokenExpiredException)
        {
            return (null, true); // Token expired but valid structure
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid token validation attempt");
            return (null, false); // Invalid token
        }
    }
}