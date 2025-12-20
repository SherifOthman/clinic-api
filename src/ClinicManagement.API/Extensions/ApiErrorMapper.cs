using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.API.Extensions;

public static class ApiErrorMapper
{
    public static ApiError ToApiError(this Result result)
    {
        var message = result.Message ?? "An error occurred";
        
        // Group errors by field and take the first message for each field to avoid duplicate keys
        var fieldErrors = result.Errors?
            .GroupBy(e => e.Field)
            .ToDictionary(g => g.Key, g => g.First().Message);
        
        return fieldErrors?.Any() == true 
            ? new ApiError(message, fieldErrors)
            : new ApiError(message);
    }
}