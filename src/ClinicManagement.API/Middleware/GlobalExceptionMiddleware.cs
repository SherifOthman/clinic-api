using ClinicManagement.API.Models;
using System.Text.Json;

namespace ClinicManagement.API.Middleware;

/// <summary>
/// Global exception handler - catches unhandled exceptions and returns RFC 7807 Problem Details
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {ExceptionType} - {Message}", 
                ex.GetType().Name, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var problemDetails = exception switch
        {
            FluentValidation.ValidationException validationEx => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400,
                Detail = "One or more validation errors occurred",
                Errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    ),
                TraceId = context.TraceIdentifier
            },
            
            UnauthorizedAccessException => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                Title = "Forbidden",
                Status = 403,
                Detail = exception.Message,
                TraceId = context.TraceIdentifier
            },
            
            KeyNotFoundException => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Title = "Not Found",
                Status = 404,
                Detail = exception.Message,
                TraceId = context.TraceIdentifier
            },
            
            InvalidOperationException => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = exception.Message,
                TraceId = context.TraceIdentifier
            },
            
            ArgumentException => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = exception.Message,
                TraceId = context.TraceIdentifier
            },
            
            _ => new ApiProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Title = "Internal Server Error",
                Status = 500,
                Detail = "An unexpected error occurred. Please try again later.",
                TraceId = context.TraceIdentifier
            }
        };

        context.Response.StatusCode = problemDetails.Status;

        var jsonResponse = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(jsonResponse);
    }
}
