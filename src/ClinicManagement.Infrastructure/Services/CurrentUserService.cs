using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
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

    public string IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "unknown";

            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
                return xForwardedFor.Split(',')[0].Trim();

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    public string? UserAgent => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();

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
            throw new UnauthorizedAccessException("Clinic ID is required but not available");

        return ClinicId.Value;
    }

    public bool TryGetUserId(out int userId)
    {
        userId = 0;
        if (UserId.HasValue)
        {
            userId = UserId.Value;
            return true;
        }
        return false;
    }

    public bool TryGetClinicId(out int clinicId)
    {
        clinicId = 0;
        if (ClinicId.HasValue)
        {
            clinicId = ClinicId.Value;
            return true;
        }
        return false;
    }
}

