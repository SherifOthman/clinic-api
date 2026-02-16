using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Common.Models;
using System.Globalization;

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
            // All domain exceptions use their ErrorCode property
            DomainException domainEx => Results.BadRequest(new ApiProblemDetails
            {
                Code = domainEx.ErrorCode,
                Title = GetTitleFromErrorCode(domainEx.ErrorCode),
                Status = 400,
                Detail = domainEx.Message,
                Data = ExtractExceptionData(domainEx),
                TraceId = traceId
            }),

            _ => throw ex
        };
    }

    /// <summary>
    /// Converts error code to human-readable title
    /// Example: "INSUFFICIENT_STOCK" → "Insufficient Stock"
    /// </summary>
    private static string GetTitleFromErrorCode(string errorCode)
    {
        if (string.IsNullOrEmpty(errorCode))
            return "Operation Failed";

        // Split by underscore and convert to title case
        var words = errorCode.Split('_');
        var titleWords = words.Select(word => 
            CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLower())
        );
        
        return string.Join(" ", titleWords);
    }

    /// <summary>
    /// Extracts additional data from exception's Data dictionary for frontend interpolation
    /// Example: { "available": 5, "requested": 10 }
    /// </summary>
    private static Dictionary<string, object>? ExtractExceptionData(DomainException exception)
    {
        if (exception.Data.Count == 0)
            return null;

        var data = new Dictionary<string, object>();
        foreach (var key in exception.Data.Keys)
        {
            if (key is string stringKey && exception.Data[key] is not null)
            {
                data[stringKey] = exception.Data[key]!;
            }
        }

        return data.Count > 0 ? data : null;
    }
}
