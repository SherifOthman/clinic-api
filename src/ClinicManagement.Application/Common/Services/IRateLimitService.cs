namespace ClinicManagement.Application.Common.Services;

public interface IRateLimitService
{
    Task<bool> IsRateLimitExceededAsync(string ipAddress, Guid? userId = null, CancellationToken cancellationToken = default);
}
