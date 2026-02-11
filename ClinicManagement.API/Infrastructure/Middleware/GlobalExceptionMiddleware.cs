using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Common.Constants;
using System.Text.Json;

namespace ClinicManagement.API.Infrastructure.Middleware;

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

        var (statusCode, code) = exception switch
        {
            DomainValidationException => (400, "VALIDATION_ERROR"),
            UnauthorizedAccessException => (403, "UNAUTHORIZED_ACCESS"),
            KeyNotFoundException => (404, "NOT_FOUND"),
            InvalidOperationException => (400, "OPERATION_NOT_ALLOWED"),
            ArgumentException => (400, "INVALID_ARGUMENT"),
            _ => (500, "INTERNAL_ERROR")
        };

        context.Response.StatusCode = statusCode;

        var response = new ApiError(code);
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(jsonResponse);
    }
}
