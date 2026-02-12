using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.API.Infrastructure.Filters;

/// <summary>
/// Endpoint filter that validates request models and returns ApiProblemDetails on validation failure
/// This ensures consistent error format between manual validation and Data Annotation validation
/// </summary>
public class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Validate each argument using Data Annotations
        var errors = new Dictionary<string, string[]>();

        foreach (var argument in context.Arguments)
        {
            if (argument is null)
                continue;

            var validationContext = new ValidationContext(argument);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(argument, validationContext, validationResults, validateAllProperties: true))
            {
                foreach (var validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        if (!errors.ContainsKey(memberName))
                        {
                            errors[memberName] = new[] { validationResult.ErrorMessage ?? "Validation error" };
                        }
                    }
                }
            }
        }

        // If there are validation errors, return ApiProblemDetails
        if (errors.Any())
        {
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.VALIDATION_ERROR,
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred",
                Errors = errors,
                TraceId = context.HttpContext.TraceIdentifier
            });
        }

        // Continue to the endpoint
        return await next(context);
    }
}
