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

        var apiError = new ApiError();

        switch (exception)
        {
          
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                apiError.Type = "ValidationError";
                apiError.Message = Result.VALIDATION_MESSAGE;
                apiError.Errors = validationEx.Errors.ToErrorItemList();
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                apiError.Type = "ServerError";
                apiError.Message = "An unexpected error occurred.";
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(apiError, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(jsonResponse);
    }

 
}
