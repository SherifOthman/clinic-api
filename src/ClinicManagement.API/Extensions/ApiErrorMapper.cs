using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.API.Extensions;

public static class ApiErrorMapper
{
    public static ApiError ToApiError(this Result result)
    {
        var code = result.Code ?? "GENERAL_ERROR";
        
        return result.ValidationErrors?.Any() == true 
            ? new ApiError(code, result.ValidationErrors.SelectMany(kvp => 
                kvp.Value.Select(msg => new ErrorItem(kvp.Key, msg))).ToList())
            : new ApiError(code);
    }
}
