using ClinicManagement.Application.Common.Constants;

namespace ClinicManagement.Application.Common.Models;

public class Result
{
    public const string VALIDATION_MESSAGE =
        "One or more fields are invalid. Please correct the errors and try again.";

    public bool Success { get; protected set; }
    public string? Code { get; protected set; }
    public string? Message { get; protected set; }
    public IEnumerable<ErrorItem>? Errors { get; protected set; }

    public static Result Ok() => new() { Success = true };

    public static Result Fail(string message) =>
        new() { Success = false, Message = message };

    public static Result Fail(string code, string message) =>
        new() { Success = false, Code = code, Message = message };

    public static Result Fail(params IEnumerable<ErrorItem> errors) =>
        new()
        {
            Success = false,
            Code = ErrorCodes.ValidationFailed,
            Message = VALIDATION_MESSAGE,
            Errors = errors
        };

    public static Result FailField(string field, string code, string message) =>
 Result.Fail(new ErrorItem { Field = field, Code = code, Message = message });


}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    public static Result<T> Ok(T value) =>
        new() { Success = true, Value = value };

    public new static Result<T> Fail(string message) =>
        new() { Success = false, Message = message };

    public new  static Result<T> Fail(string code, string message) =>
        new() { Success = false, Code = code, Message = message };

    public new static Result<T> Fail(params IEnumerable<ErrorItem> errors) =>
        new()
        {
            Success = false,
            Code = ErrorCodes.ValidationFailed,
            Message = VALIDATION_MESSAGE,
            Errors = errors
        };

    public  new static Result<T> FailField(string field, string code, string message) =>
     Result<T>.Fail(new ErrorItem { Field = field, Code = code, Message = message });

    public static implicit operator Result<T>(T value) => Ok(value);
}
