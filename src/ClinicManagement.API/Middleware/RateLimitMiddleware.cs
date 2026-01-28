using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;

namespace ClinicManagement.API.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;

    public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService, ICurrentUserService currentUserService)
    {
        if (await rateLimitService.IsRateLimitExceededAsync(currentUserService.IpAddress, currentUserService.UserId))
        {
            _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}, User: {UserId}", 
                currentUserService.IpAddress, currentUserService.UserId);
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await _next(context);
    }
}