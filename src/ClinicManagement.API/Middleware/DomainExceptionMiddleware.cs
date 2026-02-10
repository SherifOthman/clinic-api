using ClinicManagement.API.Models;
using ClinicManagement.Domain.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace ClinicManagement.API.Middleware;

/// <summary>
/// Middleware to handle domain exceptions and convert them to proper API responses
/// </summary>
public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;

    public DomainExceptionMiddleware(RequestDelegate next, ILogger<DomainExceptionMiddleware> logger)
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
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception occurred: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            await HandleDomainExceptionAsync(context, ex);
        }
        // Let other exceptions pass through to GlobalExceptionMiddleware
    }

    private static async Task HandleDomainExceptionAsync(HttpContext context, DomainException ex)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = ex switch
        {
            InsufficientStockException => HttpStatusCode.BadRequest,
            InvalidDiscountException => HttpStatusCode.BadRequest,
            ExpiredMedicineException => HttpStatusCode.BadRequest,
            DiscontinuedMedicineException => HttpStatusCode.BadRequest,
            InvalidInvoiceStateException => HttpStatusCode.BadRequest,
            InvalidAppointmentStateException => HttpStatusCode.BadRequest,
            InvalidPaymentStateException => HttpStatusCode.BadRequest,
            InvalidBusinessOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.BadRequest
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new ApiError(ex.ErrorCode);
        
        // Add exception details if available
        var details = GetExceptionDetails(ex);
        if (details != null)
        {
            // Create a response with additional details
            var detailedResponse = new
            {
                code = ex.ErrorCode,
                details = details
            };
            
            var jsonResponse = JsonSerializer.Serialize(detailedResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
        else
        {
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    private static object? GetExceptionDetails(DomainException ex)
    {
        return ex switch
        {
            InsufficientStockException stockEx => new
            {
                RequestedQuantity = stockEx.RequestedQuantity,
                AvailableQuantity = stockEx.AvailableQuantity
            },
            InvalidDiscountException discountEx => new
            {
                DiscountAmount = discountEx.DiscountAmount,
                SubtotalAmount = discountEx.SubtotalAmount
            },
            ExpiredMedicineException expiredEx => new
            {
                ExpiryDate = expiredEx.ExpiryDate
            },
            InvalidInvoiceStateException invoiceStateEx => new
            {
                CurrentStatus = invoiceStateEx.CurrentStatus.ToString(),
                Operation = invoiceStateEx.Operation
            },
            InvalidAppointmentStateException appointmentStateEx => new
            {
                CurrentStatus = appointmentStateEx.CurrentStatus.ToString(),
                Operation = appointmentStateEx.Operation
            },
            InvalidPaymentStateException paymentStateEx => new
            {
                CurrentStatus = paymentStateEx.CurrentStatus.ToString(),
                Operation = paymentStateEx.Operation
            },
            _ => null
        };
    }
}
