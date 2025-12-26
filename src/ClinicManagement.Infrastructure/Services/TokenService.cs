using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ClinicManagement.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;

    public TokenService(IUnitOfWork unitOfWork, IOptions<JwtOptions> options)
    {
        _unitOfWork = unitOfWork;
        _jwtOptions = options.Value;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles, int? clinicId = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: GetClaims(user, roles, clinicId),
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(User user,CancellationToken cancellationToken = default)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            UserId = user.Id,
        };

        _unitOfWork.RefreshTokens.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

        if (token == null)
            return;

        token.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(token!);

       await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

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
                ClockSkew = TimeSpan.Zero // No tolerance for expiration
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            // Token is expired - return null to trigger refresh
            return null;
        }
        catch (Exception)
        {
            // Token is invalid
            return null;
        }
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
            return null;

        return refreshToken;
    }

    private List<Claim> GetClaims(User user, IEnumerable<string> roles, int? clinicId)
    {
        List<Claim> claims = new List<Claim>();

        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        
        if (clinicId.HasValue)
        {
            claims.Add(new Claim("ClinicId", clinicId.Value.ToString()));
        }
        
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }
}
