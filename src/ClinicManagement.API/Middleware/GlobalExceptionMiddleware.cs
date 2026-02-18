using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
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
                Code = ErrorCodes.VALIDATION_ERROR,
                Title = "Validation Error",
                Status = 400,
                Detail = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage)),
                TraceId = context.TraceIdentifier
            },
            
            UnauthorizedAccessException => new ApiProblemDetails
            {
                Code = ErrorCodes.ACCESS_DENIED,
                Title = "Access Denied",
                Status = 403,
                Detail = exception.Message,
                TraceId = context.TraceIdentifier
            },
            
            KeyNotFoundException => new ApiProblemDetails
            {
                Code = ErrorCodes.RESOURCE_NOT_FOUND,
                Title = "Resource Not Found",
                Status = 404,
                Detail = exception.Message,
                TraceId = context.TraceIdentifier
            },
            
            InvalidOperationException => new ApiProblemDetails
            {
                Code = ErrorCodes.OPERATION_NOT_ALLOWED,
                Title = "Operation Not Allowed",
                Status = 400,
                Detail = exception.Message,
                TraceId = context.TraceIdentifier
            },
            
            ArgumentException => new ApiProblemDetails
            {
                Code = ErrorCodes.VALIDATION_ERROR,
                Title = "Validation Error",
                Status = 400,
                Detail = exception.Message,
                TraceId = context.TraceIdentifier
            },
            
            _ => new ApiProblemDetails
            {
                Code = ErrorCodes.INTERNAL_ERROR,
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
