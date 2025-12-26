using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.API.Extensions;

public static class ApiErrorMapper
{
    public static ApiError ToApiError(this Result result)
    {
        var message = result.Message ?? "An error occurred";
        
        return result.Errors?.Any() == true 
            ? new ApiError(message, result.Errors.ToList())
            : new ApiError(message);
    }
}