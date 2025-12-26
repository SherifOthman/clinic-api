using ClinicManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ClinicManagement.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public int? ClinicId
    {
        get
        {
            var clinicIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("ClinicId")?.Value;
            return int.TryParse(clinicIdClaim, out var clinicId) ? clinicId : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(x => x.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public int GetRequiredUserId()
    {
        if (!UserId.HasValue)
            throw new UnauthorizedAccessException("User ID is required but not available");

        return UserId.Value;
    }

    public int GetRequiredClinicId()
    {
        if (!ClinicId.HasValue)
            throw new UnauthorizedAccessException("Clinic ID is required but not available. User may need to complete onboarding.");

        return ClinicId.Value;
    }

    public void EnsureAuthenticated()
    {
        if (!IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated");
    }

    public void EnsureClinicAccess()
    {
        EnsureAuthenticated();
        GetRequiredClinicId(); // This will throw if no clinic access
    }
}