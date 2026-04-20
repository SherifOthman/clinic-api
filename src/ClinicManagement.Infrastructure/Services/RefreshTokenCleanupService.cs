using ClinicManagement.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
namespace ClinicManagement.Infrastructure.Services;

public class RefreshTokenCleanupService
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IRefreshTokenService refreshTokenService,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _refreshTokenService = refreshTokenService;
        _logger              = logger;
    }

    public async Task ExecuteAsync()
    {
        var count = await _refreshTokenService.CleanupExpiredTokensAsync();

        if (count > 0)
            _logger.LogInformation("Refresh token cleanup: {Count} tokens removed", count);
        else
            _logger.LogInformation("Refresh token cleanup: no expired tokens found");
    }
}
