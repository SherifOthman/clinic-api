using System.Security.Claims;

namespace ClinicManagement.API.Common.Models;

public class AccessTokenValidationResult
{
    public ClaimsPrincipal? Principal { get; private set; }
    public bool IsValid { get; private set; }
    public bool IsExpired { get; private set; }

    private AccessTokenValidationResult() { }

    public static AccessTokenValidationResult Valid(ClaimsPrincipal principal)
    {
        return new AccessTokenValidationResult
        {
            Principal = principal,
            IsValid = true,
            IsExpired = false
        };
    }

    public static AccessTokenValidationResult Expired()
    {
        return new AccessTokenValidationResult
        {
            Principal = null,
            IsValid = false,
            IsExpired = true
        };
    }

    public static AccessTokenValidationResult Invalid()
    {
        return new AccessTokenValidationResult
        {
            Principal = null,
            IsValid = false,
            IsExpired = false
        };
    }
}
