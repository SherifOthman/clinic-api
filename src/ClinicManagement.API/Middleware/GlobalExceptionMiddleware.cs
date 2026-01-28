using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Constants;
using FluentValidation;
using System.Text.Json;

namespace ClinicManagement.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, code) = exception switch
        {
            ValidationException => (400, MessageCodes.Exception.VALIDATION_ERROR),
            UnauthorizedAccessException => (403, MessageCodes.Exception.UNAUTHORIZED_ACCESS),
            KeyNotFoundException => (404, MessageCodes.Exception.NOT_FOUND),
            InvalidOperationException => (400, MessageCodes.Exception.OPERATION_NOT_ALLOWED),
            ArgumentException => (400, MessageCodes.Exception.INVALID_ARGUMENT),
            _ => (500, MessageCodes.Exception.INTERNAL_ERROR)
        };

        context.Response.StatusCode = statusCode;

        var response = new { code };
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(jsonResponse);
    }
}
