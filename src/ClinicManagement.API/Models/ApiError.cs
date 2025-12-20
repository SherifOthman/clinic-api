namespace ClinicManagement.API.Models;

/// <summary>
/// Standardized API error response
/// </summary>
public class ApiError
{
    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Field-specific errors (for validation)
    /// </summary>
    public Dictionary<string, string>? FieldErrors { get; set; }

    /// <summary>
    /// Additional error details (optional)
    /// </summary>
    public object? Details { get; set; }

    public ApiError(string message)
    {
        Message = message;
    }

    public ApiError(string message, Dictionary<string, string> fieldErrors)
    {
        Message = message;
        FieldErrors = fieldErrors;
    }
}

/// <summary>
/// Standardized API response wrapper
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response data (if successful)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error information (if failed)
    /// </summary>
    public ApiError? Error { get; set; }

    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ApiError(message)
        };
    }

    public static ApiResponse<T> Fail(string message, Dictionary<string, string> fieldErrors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ApiError(message, fieldErrors)
        };
    }
}
