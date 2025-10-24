namespace ClinicManagement.Application.Common.Models;

public class ErrorItem
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
}
public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public IEnumerable<ErrorItem>? Errors { get; protected set; }
    public string? Error { get; protected set; }


    public static Result Success() => new Result { IsSuccess = true };
    public static Result Failure(string error)
        => new Result {
            IsSuccess = false,
            Error = error};
    public static Result Failure(params IEnumerable<ErrorItem> errors) =>
        new Result {
            IsSuccess = false,
            Error = "Validation Error",
            Errors = errors};

}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    public static Result<T> Success(T value) => 
        new Result<T> { IsSuccess = true, Value = value };

    public new static Result<T> Failure(string error) =>
        new Result<T> {
            IsSuccess = false,
            Error = error };

    public static Result<T> Failure(params List<ErrorItem> errors) =>
     new Result<T>
     {
         IsSuccess = false,
         Error = "Validation Error",
         Errors = errors
     };

    public static implicit operator Result<T>(T value)
    {
        return Result<T>.Success(value);
    }
}
