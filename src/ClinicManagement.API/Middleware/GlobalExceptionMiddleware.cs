using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ClinicManagement.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly AppOptions _appOptions;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IOptions<AppOptions> appOptions)
    {
        _next       = next;
        _logger     = logger;
        _appOptions = appOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AuthenticationFailureException ex)
        {
            // OAuth callback failures redirect to login — not a JSON error response
            _logger.LogWarning(ex, "OAuth authentication failure: {Message}", ex.Message);

            if (!context.Response.HasStarted)
            {
                var loginUrl = $"{_appOptions.WebsiteUrl.TrimEnd('/')}/en/login";
                context.Response.Redirect($"{loginUrl}?error=oauth_failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    // ── Exception → HTTP status mapping ──────────────────────────────────────
    //
    // Chain of Responsibility: each entry is checked in order.
    // Adding a new exception type = one new entry here, no method changes (OCP).
    // Order matters — more specific types must come before their base types.

    private static readonly IReadOnlyList<ExceptionHandler> ExceptionHandlers =
    [
        new(
            ex => ex is FluentValidation.ValidationException,
            (ex, traceId) =>
            {
                var validationEx = (FluentValidation.ValidationException)ex;
                return new ApiProblemDetails
                {
                    Type    = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Title   = "One or more validation errors occurred.",
                    Status  = StatusCodes.Status400BadRequest,
                    Detail  = "One or more validation errors occurred",
                    Errors  = validationEx.Errors
                                .GroupBy(e => e.PropertyName)
                                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                    TraceId = traceId,
                };
            }),

        new(
            ex => ex is UnauthorizedAccessException,
            (ex, traceId) => new ApiProblemDetails
            {
                Type    = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                Title   = "Forbidden",
                Status  = StatusCodes.Status403Forbidden,
                Detail  = ex.Message,
                TraceId = traceId,
            }),

        new(
            ex => ex is KeyNotFoundException,
            (ex, traceId) => new ApiProblemDetails
            {
                Type    = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Title   = "Not Found",
                Status  = StatusCodes.Status404NotFound,
                Detail  = ex.Message,
                TraceId = traceId,
            }),

        new(
            ex => ex is DbUpdateException,
            (_, traceId) => new ApiProblemDetails
            {
                Type    = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Title   = "Database Error",
                Status  = StatusCodes.Status500InternalServerError,
                // Never expose raw SQL error messages — they can leak schema details
                Detail  = "A database error occurred. Please try again later.",
                TraceId = traceId,
            }),

        // InvalidOperationException and ArgumentException both map to 400.
        // They come after DbUpdateException (which is an InvalidOperationException subtype).
        new(
            ex => ex is InvalidOperationException or ArgumentException,
            (ex, traceId) => new ApiProblemDetails
            {
                Type    = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title   = "Bad Request",
                Status  = StatusCodes.Status400BadRequest,
                Detail  = ex.Message,
                TraceId = traceId,
            }),
    ];

    private static readonly ApiProblemDetails FallbackDetails = new()
    {
        Type   = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        Title  = "Internal Server Error",
        Status = StatusCodes.Status500InternalServerError,
        Detail = "An unexpected error occurred. Please try again later.",
    };

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var handler = ExceptionHandlers.FirstOrDefault(h => h.CanHandle(exception));

        ApiProblemDetails details;
        if (handler is not null)
        {
            details = handler.Build(exception, context.TraceIdentifier);
        }
        else
        {
            details = new ApiProblemDetails
            {
                Type    = FallbackDetails.Type,
                Title   = FallbackDetails.Title,
                Status  = FallbackDetails.Status,
                Detail  = FallbackDetails.Detail,
                TraceId = context.TraceIdentifier,
            };
        }

        context.Response.StatusCode = details.Status;

        return context.Response.WriteAsync(JsonSerializer.Serialize(details, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        }));
    }

    // ── Value object ──────────────────────────────────────────────────────────

    /// <summary>
    /// Pairs a predicate (can this handler deal with this exception?) with a
    /// factory (build the problem details). Adding a new exception type is a
    /// new entry in ExceptionHandlers — this class never changes (OCP).
    /// </summary>
    private sealed record ExceptionHandler(
        Func<Exception, bool> CanHandle,
        Func<Exception, string, ApiProblemDetails> Build);
}
