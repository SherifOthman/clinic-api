using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClinicManagement.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IOptions<JwtOptions> jwtOptions, 
        DateTimeProvider dateTimeProvider,
        ILogger<TokenService> logger)
    {
        _jwtOptions = jwtOptions.Value;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles, Guid? clinicId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (clinicId.HasValue)
        {
            claims.Add(new Claim("ClinicId", clinicId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogDebug("Generated access token for user {UserId} with clinic {ClinicId} and roles [{Roles}]", 
            user.Id, clinicId, string.Join(", ", roles));
            
        return tokenString;
    }
}

