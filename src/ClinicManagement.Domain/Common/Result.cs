namespace ClinicManagement.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }

    protected Result(bool isSuccess, string? errorCode, string? errorMessage, Dictionary<string, string[]>? validationErrors = null)
    {
        if (isSuccess && (errorCode != null || errorMessage != null))
            throw new InvalidOperationException("Successful result cannot have error details");
        
        if (!isSuccess && errorCode == null)
            throw new InvalidOperationException("Failed result must have an error code");

        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        ValidationErrors = validationErrors;
    }

    public static Result Success() => new(true, null, null);
    
    public static Result Failure(string errorCode, string errorMessage) => 
        new(false, errorCode, errorMessage);

    public static Result ValidationFailure(string errorCode, string errorMessage, Dictionary<string, string[]> validationErrors) =>
        new(false, errorCode, errorMessage, validationErrors);

    public static Result<T> Success<T>(T value) => new(value, true, null, null);
    
    public static Result<T> Failure<T>(string errorCode, string errorMessage) => 
        new(default, false, errorCode, errorMessage);

    public static Result<T> ValidationFailure<T>(string errorCode, string errorMessage, Dictionary<string, string[]> validationErrors) =>
        new(default, false, errorCode, errorMessage, validationErrors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? errorCode, string? errorMessage, Dictionary<string, string[]>? validationErrors = null)
        : base(isSuccess, errorCode, errorMessage, validationErrors)
    {
        Value = value;
    }
}
