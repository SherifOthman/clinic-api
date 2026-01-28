using ClinicManagement.API.Models;
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

        var (statusCode, message) = exception switch
        {
            ValidationException => (400, "Please check your input and try again."),
            UnauthorizedAccessException => (403, "You don't have permission to access this resource."),
            KeyNotFoundException => (404, "The requested item was not found."),
            InvalidOperationException => (400, "This operation is not allowed right now."),
            ArgumentException => (400, "Invalid information provided."),
            _ => (500, "Something went wrong. Please try again later.")
        };

        context.Response.StatusCode = statusCode;

        var response = new { message };
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(jsonResponse);
    }
}
