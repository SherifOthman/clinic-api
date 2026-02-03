using ClinicManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ClinicManagement.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationDbContext _context;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
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
            if (!UserId.HasValue) return null;
            
            // Get ClinicId from Staff entity for staff members
            var staff = _context.Staff
                .Where(s => s.UserId == UserId.Value && s.IsActive)
                .FirstOrDefault();
                
            return staff?.ClinicId;
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

            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
                return xRealIp;

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
            throw new UnauthorizedAccessException("Clinic ID is required but not available. User must be a staff member.");

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
