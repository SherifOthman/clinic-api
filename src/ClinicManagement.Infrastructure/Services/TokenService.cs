using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
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
    private readonly JwtOption _jwtOption;

    public TokenService(IConfiguration configuration,
        IUnitOfWork unitOfWork,
        IOptions<JwtOption> options)
    {
        _unitOfWork = unitOfWork;
        _jwtOption = options.Value;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOption.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOption.Issuer,
            audience: _jwtOption.Audience,
            claims: GetClaims(user, roles),
            expires: DateTime.UtcNow.AddMinutes(_jwtOption.AccessTokenExpirationMinutes),
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
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOption.RefreshTokenExpirationDays),
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

    private List<Claim> GetClaims(User user, IEnumerable<string> roles)
    {
        List<Claim> claims = new List<Claim>();

        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }
}
