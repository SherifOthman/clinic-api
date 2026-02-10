using ClinicManagement.Domain.Common.Constants;

namespace ClinicManagement.Application.Common.Models;

public class Result
{
    public const string VALIDATION_CODE = MessageCodes.Validation.GENERAL_VALIDATION_ERROR;

    public bool Success { get; protected set; }
    public string? Code { get; protected set; }
    public IEnumerable<ErrorItem>? Errors { get; protected set; }

    public bool IsFailure => !Success;

    public static Result Ok() => new() { Success = true };

    public static Result Fail(string code) =>
        new() { Success = false, Code = code };

    public static Result Fail(params IEnumerable<ErrorItem> errors) =>
        new()
        {
            Success = false,
            Code = VALIDATION_CODE,
            Errors = errors
        };

    public static Result FailField(string field, string code) =>
        Result.Fail(new ErrorItem(field, code));
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    public static Result<T> Ok(T value) =>
        new() { Success = true, Value = value };

    public new static Result<T> Fail(string code) =>
        new() { Success = false, Code = code };

    public new static Result<T> Fail(params IEnumerable<ErrorItem> errors) =>
        new()
        {
            Success = false,
            Code = VALIDATION_CODE,
            Errors = errors
        };

    public new static Result<T> FailField(string field, string code) =>
        Result<T>.Fail(new ErrorItem(field, code));

    public static implicit operator Result<T>(T value) => Ok(value);
}
