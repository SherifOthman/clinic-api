using ClinicManagement.Application.Common.Models;
using FluentValidation.Results;

namespace ClinicManagement.Application.Extensions;

public static class ErrorItemMapper
{
    public static IEnumerable<ErrorItem> ToErrorItemList(this IEnumerable<ValidationFailure> failures)
    {
        return failures.Select(f => new ErrorItem
        {
            Field = ToCamelCase(f.PropertyName),
            Message = f.ErrorMessage
        });
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            return input;

        return char.ToLowerInvariant(input[0]) + input[1..];
    }
}
