using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.API.Extensions;

public static class ApiErrorMapper
{
    public static ApiError ToApiError(this Result result)
    {
        return new ApiError
        {
            Type = result.Errors != null ? "ValidationError" : "BusinessError",
            Code = result.Code,
            Message = result.Message ?? "An error occurred",
            Errors = result.Errors
        };
    }
}