using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using System.Security.Claims;

namespace ClinicManagement.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, Guid? clinicId = null);
    
    ClaimsPrincipal? ValidateAccessToken(string token);
    
    bool IsTokenExpired(string token);

    AccessTokenValidationResult ValidateAccessTokenWithExpiry(string token);
}