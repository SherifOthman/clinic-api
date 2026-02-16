using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ClinicManagement.API.Infrastructure.Services;

public class CurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public Guid? ClinicId
    {
        get
        {
            var clinicIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("ClinicId")?.Value;
            return Guid.TryParse(clinicIdClaim, out var clinicId) ? clinicId : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "unknown";

            // Check X-Forwarded-For header first (for proxied requests)
            // Takes first IP in chain as that's the original client IP
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
                return xForwardedFor.Split(',')[0].Trim();

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    public string? UserAgent => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(x => x.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Guid GetRequiredUserId()
    {
        if (!UserId.HasValue)
            throw new UnauthorizedAccessException("User ID is required but not available");

        return UserId.Value;
    }

    public Guid GetRequiredClinicId()
    {
        if (!ClinicId.HasValue)
            throw new UnauthorizedAccessException("Clinic ID is required but not available");

        return ClinicId.Value;
    }

    public bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        if (UserId.HasValue)
        {
            userId = UserId.Value;
            return true;
        }
        return false;
    }

    public bool TryGetClinicId(out Guid clinicId)
    {
        clinicId = Guid.Empty;
        if (ClinicId.HasValue)
        {
            clinicId = ClinicId.Value;
            return true;
        }
        return false;
    }
}
