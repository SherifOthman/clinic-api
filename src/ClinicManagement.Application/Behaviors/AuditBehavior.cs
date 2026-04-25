using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Behaviors;

/// <summary>
/// Pipeline behavior that writes an AuditLog entry after any command that:
///   1. Implements IAuditableCommand
///   2. Returns a successful Result or Result&lt;T&gt;
///
/// Runs after the handler — never blocks on failure.
/// Login/logout/lockout events stay manual in LoginHandler (they audit failures too).
/// </summary>
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IAuditableCommand
{
    private readonly ISecurityAuditWriter _auditWriter;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;

    public AuditBehavior(
        ISecurityAuditWriter auditWriter,
        ICurrentUserService currentUser,
        ILogger<AuditBehavior<TRequest, TResponse>> logger)
    {
        _auditWriter = auditWriter;
        _currentUser = currentUser;
        _logger      = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        // Only audit on success
        var isSuccess = response switch
        {
            Result r    => r.IsSuccess,
            _           => true, // non-Result responses are always considered success
        };

        if (!isSuccess) return response;

        try
        {
            await _auditWriter.WriteAsync(
                _currentUser.UserId,
                _currentUser.FullName,
                _currentUser.Username,
                _currentUser.Email,
                _currentUser.Roles.FirstOrDefault(),
                _currentUser.ClinicId,
                request.AuditEvent,
                request.AuditDetail,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Audit failure must never break the request
            _logger.LogError(ex, "AuditBehavior failed for {Event}", request.AuditEvent);
        }

        return response;
    }
}
