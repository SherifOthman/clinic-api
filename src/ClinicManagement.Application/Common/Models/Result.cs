using System.Text.Json.Serialization;

namespace ClinicManagement.Application.Common.Models;
public class Result
{
    public bool Success { get; protected set; }

    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; protected set; }

    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<ErrorItem>? Errors { get; protected set; }


    public static Result Ok() => new Result { Success = true };
    public static Result Fail(string error)
        => new Result {
            Success = false,
            Message = error};
    public static Result Fail(params IEnumerable<ErrorItem> errors) =>
        new Result {
            Success = false,
            Message = "One or more fields are invalid. Please correct the errors and try again.",
            Errors = errors};

}

public class Result<T> : Result
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
   
    public T? Value { get; private set; }

    public  static Result<T> Ok(T value) => 
        new Result<T> { Success = true, Value = value };

    public new static Result<T> Fail(string error) =>
        new Result<T> {
            Success = false,
            Message = error };

    public static Result<T> Fail(params List<ErrorItem> errors) =>
     new Result<T>
     {
         Success = false,
         Message = "One or more fields are invalid. Please correct the errors and try again.",
         Errors = errors
     };

    public static implicit operator Result<T>(T value)
    {
        return Result<T>.Ok(value);
    }
}
