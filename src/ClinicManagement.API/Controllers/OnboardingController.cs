using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Auth.Commands;
using ClinicManagement.Application.Features.Onboarding.Commands;
using ClinicManagement.Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Controllers;

[Authorize]
[Route("api/onboarding")]
public class OnboardingController : BaseApiController
{
    private readonly ICookieService _cookieService;
    private readonly int _accessTokenExpiryMinutes;

    public OnboardingController(ICookieService cookieService, IOptions<JwtOptions> jwtOptions)
    {
        _cookieService            = cookieService;
        _accessTokenExpiryMinutes = jwtOptions.Value.AccessTokenExpirationMinutes;
    }

    [HttpPost("complete")]
    [Authorize(Policy = "RequireClinicOwner")]
    [EnableRateLimiting(RateLimitPolicies.UserOnce)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompleteOnboarding(
        [FromBody] CompleteOnboarding request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request, cancellationToken);
        if (!result.IsSuccess) return HandleResult(result, "Onboarding Failed");

        // Re-issue tokens so the new JWT contains the ClinicId claim.
        // Without this, RequireClinicOwner endpoints return 403 until the
        // access token naturally expires and the refresh token issues a new one.
        var refreshToken = _cookieService.GetRefreshTokenFromCookie();
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var refreshResult = await Sender.Send(
                new RefreshTokenCommand(refreshToken),
                cancellationToken);

            if (refreshResult.IsSuccess && refreshResult.Value is not null)
            {
                _cookieService.SetAccessTokenCookie(refreshResult.Value.AccessToken!, _accessTokenExpiryMinutes);
                _cookieService.SetRefreshTokenCookie(refreshResult.Value.RefreshToken!);
            }
        }

        return NoContent();
    }
}
