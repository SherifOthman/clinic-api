namespace ClinicManagement.Application.Common.Services;

public interface IRateLimitService
{
    Task<bool> IsRateLimitExceededAsync(string ipAddress, int? userId = null, CancellationToken cancellationToken = default);
}
