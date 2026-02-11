using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Common.Constants;

namespace ClinicManagement.API.Common.Extensions;

/// <summary>
/// Converts domain exceptions to RFC 7807 Problem Details responses
/// All errors return standardized format with error codes for frontend translation
/// </summary>
public static class ResultExtensions
{
    public static IResult HandleDomainException(this Exception ex, HttpContext? httpContext = null)
    {
        var traceId = httpContext?.TraceIdentifier;

        return ex switch
        {
            // Validation errors - return field-level errors for form mapping
            DomainValidationException validationEx => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.VALIDATION_ERROR,
                Title = "Validation Failed",
                Status = 400,
                Detail = "One or more validation errors occurred",
                Errors = validationEx.ValidationErrors.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToArray()
                ),
                TraceId = traceId
            }),

            InvalidBusinessOperationException => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.OPERATION_NOT_ALLOWED,
                Title = "Operation Not Allowed",
                Status = 400,
                Detail = ex.Message,
                TraceId = traceId
            }),

            InvalidInvoiceStateException => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_STATE_TRANSITION,
                Title = "Invalid State Transition",
                Status = 400,
                Detail = ex.Message,
                TraceId = traceId
            }),

            InvalidAppointmentStateException => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_STATE_TRANSITION,
                Title = "Invalid State Transition",
                Status = 400,
                Detail = ex.Message,
                TraceId = traceId
            }),

            InvalidPaymentStateException => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_STATE_TRANSITION,
                Title = "Invalid State Transition",
                Status = 400,
                Detail = ex.Message,
                TraceId = traceId
            }),

            // Stock exception includes data for message interpolation
            InsufficientStockException stockEx => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INSUFFICIENT_STOCK,
                Title = "Insufficient Stock",
                Status = 400,
                Detail = ex.Message,
                Data = ExtractStockData(stockEx.Message),
                TraceId = traceId
            }),

            DiscontinuedMedicineException => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.MEDICINE_DISCONTINUED,
                Title = "Medicine Discontinued",
                Status = 400,
                Detail = ex.Message,
                TraceId = traceId
            }),

            ExpiredMedicineException => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.MEDICINE_EXPIRED,
                Title = "Medicine Expired",
                Status = 400,
                Detail = ex.Message,
                TraceId = traceId
            }),

            InvalidDiscountException => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_DISCOUNT,
                Title = "Invalid Discount",
                Status = 400,
                Detail = ex.Message,
                TraceId = traceId
            }),

            InvalidMoneyException => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_FORMAT,
                Title = "Invalid Format",
                Status = 400,
                Detail = ex.Message,
                TraceId = traceId
            }),

            InvalidEmailException => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_EMAIL,
                Title = "Invalid Email",
                Status = 400,
                Detail = ex.Message,
                TraceId = traceId
            }),

            InvalidPhoneNumberException => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_PHONE,
                Title = "Invalid Phone Number",
                Status = 400,
                Detail = ex.Message,
                TraceId = traceId
            }),

            DomainException domainEx when !string.IsNullOrEmpty(domainEx.ErrorCode) => 
                Results.BadRequest(new ApiProblemDetails
                {
                    Code = domainEx.ErrorCode,
                    Title = "Operation Failed",
                    Status = 400,
                    Detail = domainEx.Message,
                    TraceId = traceId
                }),

            DomainException domainEx => Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.OPERATION_FAILED,
                Title = "Operation Failed",
                Status = 400,
                Detail = domainEx.Message,
                TraceId = traceId
            }),

            _ => throw ex
        };
    }

    /// <summary>
    /// Extracts numeric data from error messages for frontend interpolation
    /// Example: "Available: 5, Requested: 10" → { "available": 5, "requested": 10 }
    /// </summary>
    private static Dictionary<string, object>? ExtractStockData(string message)
    {
        try
        {
            var data = new Dictionary<string, object>();
            var availableMatch = System.Text.RegularExpressions.Regex.Match(message, @"Available:\s*(\d+)");
            var requestedMatch = System.Text.RegularExpressions.Regex.Match(message, @"Requested:\s*(\d+)");

            if (availableMatch.Success)
                data["available"] = int.Parse(availableMatch.Groups[1].Value);
            
            if (requestedMatch.Success)
                data["requested"] = int.Parse(requestedMatch.Groups[1].Value);

            return data.Count > 0 ? data : null;
        }
        catch
        {
            return null;
        }
    }
}
