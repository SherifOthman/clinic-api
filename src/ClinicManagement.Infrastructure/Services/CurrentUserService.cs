using ClinicManagement.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ClinicManagement.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _http = httpContextAccessor;

    // ── Claims ────────────────────────────────────────────────────────────────

    public Guid?   UserId      => ParseGuid(ClaimTypes.NameIdentifier);
    public Guid?   MemberId    => ParseGuid("MemberId");
    public Guid?   ClinicId    => ParseGuid("ClinicId");
    public string? CountryCode => Claim("CountryCode");
    public string? FullName    => Claim(ClaimTypes.Name);
    public string? Username    => _http.HttpContext?.User?.Identity?.Name;
    public string? Email       => Claim(ClaimTypes.Email);
    public string? UserAgent   => _http.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
    public bool    IsAuthenticated => _http.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles
        => _http.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value)
           ?? Enumerable.Empty<string>();

    public string IpAddress
    {
        get
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return "unknown";

            var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
                return forwarded.Split(',')[0].Trim();

            return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    // ── Required helpers ──────────────────────────────────────────────────────

    public Guid GetRequiredUserId()
        => UserId ?? throw new UnauthorizedAccessException("User ID claim is missing.");

    public Guid GetRequiredClinicId()
        => ClinicId ?? throw new UnauthorizedAccessException("Clinic ID claim is missing.");

    // ── Private ───────────────────────────────────────────────────────────────

    private string? Claim(string type)
        => _http.HttpContext?.User?.FindFirstValue(type);

    private Guid? ParseGuid(string type)
        => Guid.TryParse(Claim(type), out var id) ? id : null;
}
