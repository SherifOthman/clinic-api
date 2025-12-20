using Azure;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Extensions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace ClinicManagement.API.Middleware;

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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        ApiError apiError;

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var fieldErrors = validationEx.Errors.ToErrorItemList().ToDictionary(e => e.Field, e => e.Message);
                apiError = new ApiError(Result.VALIDATION_MESSAGE, fieldErrors);
                break;

            case UnauthorizedAccessException unauthorizedEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                apiError = new ApiError(unauthorizedEx.Message ?? "You do not have permission to access this resource.");
                break;

            case KeyNotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                apiError = new ApiError(notFoundEx.Message ?? "The requested resource was not found.");
                break;

            case InvalidOperationException invalidOpEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                apiError = new ApiError(invalidOpEx.Message ?? "The operation is not valid.");
                break;

            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                apiError = new ApiError(argEx.Message ?? "Invalid argument provided.");
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var message = "An unexpected error occurred. Please try again later.";
                
                // In development, include exception details
                #if DEBUG
                message += $" Details: {exception.Message}";
                #endif
                
                apiError = new ApiError(message);
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(apiError, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(jsonResponse);
    }

 
}
