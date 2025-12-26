using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Utils;
using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;

namespace ClinicManagement.Application.Extensions;
public static class ErrorItemMapper
{
    public static IEnumerable<ErrorItem> ToErrorItemList(
        this IEnumerable<ValidationFailure> failures)
    {
        return failures.Select(f => new ErrorItem
        {
            Field = StringUtils.ToCamelCase(f.PropertyName),
            Message = f.ErrorMessage
        });
    }
}
