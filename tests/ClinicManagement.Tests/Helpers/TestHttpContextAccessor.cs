using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ClinicManagement.Tests.Helpers;

/// <summary>
/// Test helper class to provide a fake HttpContextAccessor for testing
/// </summary>
internal class TestHttpContextAccessor : IHttpContextAccessor
{
    private readonly Guid _clinicId;
    private readonly Guid? _userId;

    public TestHttpContextAccessor(Guid clinicId, Guid? userId = null)
    {
        _clinicId = clinicId;
        _userId = userId;
    }

    public HttpContext? HttpContext
    {
        get
        {
            var context = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("ClinicId", _clinicId.ToString())
            };

            if (_userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, _userId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            context.User = principal;
            return context;
        }
        set { }
    }
}
