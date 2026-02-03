using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.API.Extensions;

public static class ApiErrorMapper
{
    public static ApiError ToApiError(this Result result)
    {
        var code = result.Code ?? "GENERAL.ERROR";
        
        return result.Errors?.Any() == true 
            ? new ApiError(code, result.Errors.ToList())
            : new ApiError(code);
    }
}
