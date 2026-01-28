using ClinicManagement.Application.Common.Constants;

namespace ClinicManagement.Application.Common.Models;

public class Result
{
    public const string VALIDATION_MESSAGE = MessageCodes.Validation.GENERAL_VALIDATION_ERROR;

    public bool Success { get; protected set; }
    public string? Message { get; protected set; }
    public IEnumerable<ErrorItem>? Errors { get; protected set; }

    public bool IsFailure => !Success;

    public static Result Ok() => new() { Success = true };

    public static Result Fail(string message) =>
        new() { Success = false, Message = message };

    public static Result Fail(params IEnumerable<ErrorItem> errors) =>
        new()
        {
            Success = false,
            Message = VALIDATION_MESSAGE,
            Errors = errors
        };

    public static Result FailField(string field, string message) =>
        Result.Fail(new ErrorItem(field, message));
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    public static Result<T> Ok(T value) =>
        new() { Success = true, Value = value };

    public new static Result<T> Fail(string message) =>
        new() { Success = false, Message = message };

    public new static Result<T> Fail(params IEnumerable<ErrorItem> errors) =>
        new()
        {
            Success = false,
            Message = VALIDATION_MESSAGE,
            Errors = errors
        };

    public new static Result<T> FailField(string field, string message) =>
        Result<T>.Fail(new ErrorItem(field, message));

    public static implicit operator Result<T>(T value) => Ok(value);
}
