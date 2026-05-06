using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ClinicManagement.Application.Behaviors;

/// <summary>
/// Pipeline behavior that logs a warning when a request exceeds 200ms.
/// Helps identify performance bottlenecks in production.
/// Uses Stopwatch.StartNew() per invocation — safe for any registration lifetime.
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int SlowRequestThresholdMs = 200;

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        var response = await next();
        timer.Stop();

        if (timer.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "Slow request: {RequestName} took {ElapsedMilliseconds}ms (threshold: {ThresholdMs}ms)",
                typeof(TRequest).Name,
                timer.ElapsedMilliseconds,
                SlowRequestThresholdMs);
        }

        return response;
    }
}
