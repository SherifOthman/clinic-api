using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ClinicManagement.API.Common.Extensions;

/// <summary>
/// Extension methods for creating standardized ValidationProblem responses
/// All validation errors (data annotations + business logic) use the same RFC 7807 format
/// </summary>
public static class ValidationProblemExtensions
{
    /// <summary>
    /// Creates a ValidationProblem response with error code at root level for easy frontend consumption
    /// </summary>
    /// <param name="modelState">ModelState dictionary containing validation errors</param>
    /// <param name="errorCode">Error code for frontend translation (e.g., "INSUFFICIENT_STOCK")</param>
    /// <param name="httpContext">Optional HTTP context for traceId</param>
    /// <returns>BadRequest result with ValidationProblemDetails</returns>
    public static IResult ToValidationProblem(
        this ModelStateDictionary modelState, 
        string errorCode,
        HttpContext? httpContext = null)
    {
        var problemDetails = new ValidationProblemDetails(modelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Extensions = { ["code"] = errorCode }
        };

        if (httpContext != null)
        {
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        }

        return Results.BadRequest(problemDetails);
    }

    /// <summary>
    /// Adds a general error (not field-specific) to ModelState
    /// </summary>
    /// <param name="modelState">ModelState dictionary</param>
    /// <param name="errorCode">Error code for frontend translation</param>
    public static void AddGeneralError(this ModelStateDictionary modelState, string errorCode)
    {
        modelState.AddModelError(string.Empty, errorCode);
    }

    /// <summary>
    /// Adds a field-specific error to ModelState
    /// </summary>
    /// <param name="modelState">ModelState dictionary</param>
    /// <param name="fieldName">Field name (e.g., "email", "quantity")</param>
    /// <param name="errorCode">Error code for frontend translation</param>
    public static void AddFieldError(this ModelStateDictionary modelState, string fieldName, string errorCode)
    {
        modelState.AddModelError(fieldName, errorCode);
    }
}
