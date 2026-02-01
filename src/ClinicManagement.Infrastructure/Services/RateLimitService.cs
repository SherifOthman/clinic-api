using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class RateLimitService : IRateLimitService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    
    private const int IpRateLimitPerMinute = 100;
    private const int UserRateLimitPerMinute = 200; // Higher limit for authenticated users
    private const int WindowMinutes = 1;

    public RateLimitService(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<RateLimitService> logger, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> IsRateLimitExceededAsync(string ipAddress, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var identifier = userId?.ToString() ?? ipAddress;
        var type = userId.HasValue ? "USER" : "IP";
        var limit = userId.HasValue ? UserRateLimitPerMinute : IpRateLimitPerMinute;
        var cacheKey = $"rate_limit_{type}_{identifier}";

        // Try memory cache first for performance
        if (_cache.TryGetValue(cacheKey, out int cachedCount))
        {
            if (cachedCount >= limit)
            {
                _logger.LogWarning("Rate limit exceeded in cache for {Type}:{Identifier} ({Count}/{Limit})", 
                    type, identifier, cachedCount, limit);
                return true;
            }
            
            _cache.Set(cacheKey, cachedCount + 1, TimeSpan.FromMinutes(WindowMinutes));
            return false;
        }

        // Fallback to database for persistence across restarts
        try
        {
            var currentTime = _dateTimeProvider.UtcNow;
            var windowStart = currentTime.AddMinutes(-WindowMinutes);
            var entry = await _unitOfWork.RateLimitEntries.GetActiveEntryAsync(identifier, type, windowStart, cancellationToken);

            if (entry == null)
            {
                entry = new RateLimitEntry
                {
                    Identifier = identifier,
                    Type = type,
                    RequestCount = 1,
                    WindowStart = currentTime,
                    ExpiresAt = currentTime.AddMinutes(WindowMinutes)
                };
                
                await _unitOfWork.RateLimitEntries.AddAsync(entry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _cache.Set(cacheKey, 1, TimeSpan.FromMinutes(WindowMinutes));
                return false;
            }

            if (entry.RequestCount >= limit)
            {
                _logger.LogWarning("Rate limit exceeded in database for {Type}:{Identifier} ({Count}/{Limit})", 
                    type, identifier, entry.RequestCount, limit);
                return true;
            }

            entry.RequestCount++;
            await _unitOfWork.RateLimitEntries.UpdateAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _cache.Set(cacheKey, entry.RequestCount, TimeSpan.FromMinutes(WindowMinutes));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for {Type}:{Identifier}, allowing request", type, identifier);
            
            // Fallback to memory cache only on database error
            _cache.Set(cacheKey, 1, TimeSpan.FromMinutes(WindowMinutes));
            return false;
        }
    }
}