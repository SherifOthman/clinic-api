using System.Security.Claims;

namespace ClinicManagement.Domain.Common.Constants;

public static class ClaimConstants
{
    public const string UserId = ClaimTypes.NameIdentifier;
    public const string ClinicId = "ClinicId";
    public const string Email = ClaimTypes.Email;
    public const string Role = ClaimTypes.Role;
    public const string FullName = "FullName";
    public const string ClinicName = "ClinicName";

    public static readonly string[] CustomClaims = { ClinicId, FullName, ClinicName };
}