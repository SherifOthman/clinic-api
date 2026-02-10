namespace ClinicManagement.Application.Common.Models;

/// <summary>
/// Represents the result of an operation
/// Supports both validation errors (field-level) and business logic errors (with codes)
/// </summary>
public class Result
{
    public bool Success { get; protected set; }
    public string? Code { get; protected set; }
    public string? Message { get; protected set; }
    public Dictionary<string, List<string>>? ValidationErrors { get; protected set; }
    public object? Metadata { get; protected set; }

    public bool IsFailure => !Success;
    public bool HasValidationErrors => ValidationErrors?.Any() ?? false;
    public bool HasBusinessError => !string.IsNullOrEmpty(Code);

    // Backward compatibility - alias for ValidationErrors
    [Obsolete("Use ValidationErrors instead")]
    public IEnumerable<ErrorItem>? Errors
    {
        get
        {
            if (ValidationErrors == null) return null;
            return ValidationErrors.SelectMany(kvp =>
                kvp.Value.Select(msg => new ErrorItem(kvp.Key, msg)));
        }
    }

    // Success result
    public static Result Ok() => new() { Success = true };

    // Business logic error (with code and optional metadata)
    public static Result FailBusiness(string code, string message, object? metadata = null) =>
        new()
        {
            Success = false,
            Code = code,
            Message = message,
            Metadata = metadata
        };

    // System error (with code)
    public static Result FailSystem(string code, string message) =>
        new()
        {
            Success = false,
            Code = code,
            Message = message
        };

    // Validation errors (field-level, no code)
    public static Result FailValidation(Dictionary<string, List<string>> errors) =>
        new()
        {
            Success = false,
            ValidationErrors = errors
        };

    // Single field validation error
    public static Result FailValidation(string field, string message) =>
        new()
        {
            Success = false,
            ValidationErrors = new Dictionary<string, List<string>>
            {
                [field] = new List<string> { message }
            }
        };

    // Backward compatibility - deprecated
    [Obsolete("Use FailBusiness, FailSystem, or FailValidation instead")]
    public static Result Fail(string code) =>
        new() { Success = false, Code = code, Message = code };

    [Obsolete("Use FailValidation instead")]
    public static Result Fail(params IEnumerable<ErrorItem> errors)
    {
        var validationErrors = new Dictionary<string, List<string>>();
        foreach (var error in errors)
        {
            if (!validationErrors.ContainsKey(error.Field))
                validationErrors[error.Field] = new List<string>();
            validationErrors[error.Field].Add(error.Code);
        }
        return new()
        {
            Success = false,
            ValidationErrors = validationErrors
        };
    }

    [Obsolete("Use FailValidation instead")]
    public static Result FailField(string field, string code) =>
        FailValidation(field, code);
}

/// <summary>
/// Represents the result of an operation with a return value
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; private set; }

    // Backward compatibility - alias for ValidationErrors
    [Obsolete("Use ValidationErrors instead")]
    public new IEnumerable<ErrorItem>? Errors
    {
        get
        {
            if (ValidationErrors == null) return null;
            return ValidationErrors.SelectMany(kvp =>
                kvp.Value.Select(msg => new ErrorItem(kvp.Key, msg)));
        }
    }

    // Success result with value
    public static Result<T> Ok(T value) =>
        new() { Success = true, Value = value };

    // Business logic error (with code and optional metadata)
    public new static Result<T> FailBusiness(string code, string message, object? metadata = null) =>
        new()
        {
            Success = false,
            Code = code,
            Message = message,
            Metadata = metadata
        };

    // System error (with code)
    public new static Result<T> FailSystem(string code, string message) =>
        new()
        {
            Success = false,
            Code = code,
            Message = message
        };

    // Validation errors (field-level, no code)
    public new static Result<T> FailValidation(Dictionary<string, List<string>> errors) =>
        new()
        {
            Success = false,
            ValidationErrors = errors
        };

    // Single field validation error
    public new static Result<T> FailValidation(string field, string message) =>
        new()
        {
            Success = false,
            ValidationErrors = new Dictionary<string, List<string>>
            {
                [field] = new List<string> { message }
            }
        };

    // Backward compatibility - deprecated
    [Obsolete("Use FailBusiness, FailSystem, or FailValidation instead")]
    public new static Result<T> Fail(string code) =>
        new() { Success = false, Code = code, Message = code };

    [Obsolete("Use FailValidation instead")]
    public new static Result<T> Fail(params IEnumerable<ErrorItem> errors)
    {
        var validationErrors = new Dictionary<string, List<string>>();
        foreach (var error in errors)
        {
            if (!validationErrors.ContainsKey(error.Field))
                validationErrors[error.Field] = new List<string>();
            validationErrors[error.Field].Add(error.Code);
        }
        return new()
        {
            Success = false,
            ValidationErrors = validationErrors
        };
    }

    [Obsolete("Use FailValidation instead")]
    public new static Result<T> FailField(string field, string code) =>
        FailValidation(field, code);

    public static implicit operator Result<T>(T value) => Ok(value);
}
