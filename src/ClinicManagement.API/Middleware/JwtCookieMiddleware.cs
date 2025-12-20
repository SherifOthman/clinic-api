using ClinicManagement.API.Options;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Auth.Commands.RefreshToken;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClinicManagement.API.Middleware;

/// <summary>
/// Enterprise-grade JWT authentication middleware
/// Handles token validation, automatic refresh, and security hardening
/// Uses the options pattern for configuration
/// </summary>
public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtMiddlewareOptions _options;
    private readonly ILogger<JwtCookieMiddleware> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TokenValidationParameters _validationParameters;

    public JwtCookieMiddleware(
        RequestDelegate next, 
        IOptions<JwtMiddlewareOptions> options, 
        ILogger<JwtCookieMiddleware> logger)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();
        
        // Validate required options
        ValidateOptions();
        
        // Pre-configure validation parameters for performance
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = _options.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_options.Key)),
            ValidateIssuer = _options.ValidateIssuer,
            ValidIssuer = _options.Issuer,
            ValidateAudience = _options.ValidateAudience,
            ValidAudience = _options.Audience,
            ValidateLifetime = false, // We'll validate lifetime manually for precise control
            ClockSkew = _options.ClockSkew
        };

        if (_options.EnableDetailedLogging)
        {
            _logger.LogInformation("JWT Middleware initialized with options: {@Options}", new
            {
                _options.Issuer,
                _options.Audience,
                _options.ClockSkew,
                _options.EnableAutoRefresh,
                _options.ClearCookiesOnInvalidToken,
                PublicPathsCount = _options.PublicPaths.Length
            });
        }
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrEmpty(_options.Key))
            throw new InvalidOperationException("JWT Key is required");
        
        if (string.IsNullOrEmpty(_options.Issuer))
            throw new InvalidOperationException("JWT Issuer is required");
        
        if (string.IsNullOrEmpty(_options.Audience))
            throw new InvalidOperationException("JWT Audience is required");
    }

    public async Task InvokeAsync(HttpContext context, ICookieService cookieService, IMediator mediator)
    {
        // Skip public endpoints
        if (ShouldSkipAuthentication(context.Request.Path))
        {
            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug("Skipping authentication for public path: {Path}", context.Request.Path);
            }
            await _next(context);
            return;
        }

        var accessToken = cookieService.GetAccessTokenFromCookie();
        
        if (!string.IsNullOrEmpty(accessToken))
        {
            var tokenValidation = ValidateAccessToken(accessToken);
            
            if (tokenValidation.IsValid && !tokenValidation.IsExpired)
            {
                // Token is valid and not expired
                context.User = tokenValidation.Principal!;
                
                if (_options.EnableDetailedLogging)
                {
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    _logger.LogDebug("Valid access token for user: {UserId}", userId);
                }
                
                await _next(context);
                return;
            }
            
            if (tokenValidation.IsValid && tokenValidation.IsExpired && _options.EnableAutoRefresh)
            {
                // Token is valid but expired - attempt silent refresh
                var refreshSuccess = await TryRefreshTokenAsync(cookieService, mediator, context);
                if (refreshSuccess)
                {
                    await _next(context);
                    return;
                }
            }
            
            // Invalid or tampered token - clear cookies if configured
            if (!tokenValidation.IsValid && _options.ClearCookiesOnInvalidToken)
            {
                _logger.LogWarning("Invalid or tampered access token detected, clearing cookies");
                cookieService.ClearAuthCookies();
            }
        }

        // Continue without authentication - let [Authorize] handle authorization
        await _next(context);
    }

    private TokenValidationResult ValidateAccessToken(string token)
    {
        try
        {
            // Validate signature and claims (but not lifetime)
            var principal = _tokenHandler.ValidateToken(token, _validationParameters, out var validatedToken);
            
            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug("Token validation failed: Not a valid JWT token");
                }
                return new TokenValidationResult { IsValid = false };
            }

            // Manual lifetime validation for precise control with buffer time
            var now = DateTimeOffset.UtcNow;
            var isExpired = jwtToken.ValidTo <= now.DateTime;
            var isNearExpiry = jwtToken.ValidTo <= now.Add(_options.RefreshBufferTime).DateTime;

            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug("Token validation result: Valid={IsValid}, Expired={IsExpired}, NearExpiry={IsNearExpiry}, ExpiresAt={ExpiresAt}", 
                    true, isExpired, isNearExpiry, jwtToken.ValidTo);
            }

            return new TokenValidationResult 
            { 
                IsValid = true, 
                IsExpired = isExpired,
                IsNearExpiry = isNearExpiry,
                Principal = principal 
            };
        }
        catch (SecurityTokenException ex)
        {
            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug(ex, "Token validation failed: {Message}", ex.Message);
            }
            return new TokenValidationResult { IsValid = false };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error during token validation");
            return new TokenValidationResult { IsValid = false };
        }
    }

    private async Task<bool> TryRefreshTokenAsync(ICookieService cookieService, IMediator mediator, HttpContext context)
    {
        var refreshToken = cookieService.GetRefreshTokenFromCookie();
        if (string.IsNullOrEmpty(refreshToken))
        {
            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug("No refresh token available for silent refresh");
            }
            return false;
        }

        try
        {
            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug("Attempting token refresh");
            }

            var refreshResult = await mediator.Send(new RefreshTokenCommand { RefreshToken = refreshToken });
            
            if (refreshResult.Success)
            {
                // Set new tokens as secure cookies
                cookieService.SetAccessTokenCookie(refreshResult.Value!.AccessToken);
                cookieService.SetRefreshTokenCookie(refreshResult.Value!.RefreshToken);
                
                // Validate new access token and set user principal
                var newTokenValidation = ValidateAccessToken(refreshResult.Value!.AccessToken);
                if (newTokenValidation.IsValid && !newTokenValidation.IsExpired)
                {
                    context.User = newTokenValidation.Principal!;
                    
                    // Enhanced logging with user context
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    _logger.LogInformation("Successfully performed silent token refresh for user {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("New access token validation failed after refresh");
                }
            }
            else
            {
                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug("Token refresh failed: {Error}", refreshResult.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception during token refresh");
        }

        // Refresh failed - clear invalid cookies if configured
        if (_options.ClearCookiesOnInvalidToken)
        {
            _logger.LogInformation("Clearing authentication cookies due to refresh failure");
            cookieService.ClearAuthCookies();
        }
        return false;
    }

    private bool ShouldSkipAuthentication(PathString path)
    {
        return _options.PublicPaths.Any(publicPath => 
            path.StartsWithSegments(publicPath, StringComparison.OrdinalIgnoreCase));
    }

    private class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
        public bool IsNearExpiry { get; set; }
        public ClaimsPrincipal? Principal { get; set; }
    }
}