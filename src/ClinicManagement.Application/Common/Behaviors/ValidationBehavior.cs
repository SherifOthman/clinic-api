using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Extensions;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        var failures = validationResults
            .SelectMany(r => r.Errors ?? Enumerable.Empty<FluentValidation.Results.ValidationFailure>())
            .ToList();

        if (!failures.Any())
            return await next();

        // Always return Result for validation failures
        return CreateFailureResult(failures);
    }

    private static bool IsResultType(Type type) =>
        typeof(Result).IsAssignableFrom(type) ||
        (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>));

    private static TResponse CreateFailureResult(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
    {
        // Convert FluentValidation failures to validation errors dictionary
        var validationErrors = new Dictionary<string, List<string>>();
        
        foreach (var failure in failures)
        {
            var field = failure.PropertyName;
            if (!validationErrors.ContainsKey(field))
                validationErrors[field] = new List<string>();
            validationErrors[field].Add(failure.ErrorMessage);
        }

        // Handle Result<T> types using reflection (appropriate for generic pipeline behavior)
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            // For Result<T>, we need to call Result<T>.FailValidation(validationErrors)
            var resultType = typeof(TResponse);
            var failMethod = resultType.GetMethod("FailValidation", new[] { typeof(Dictionary<string, List<string>>) });
            
            if (failMethod != null)
            {
                return (TResponse)failMethod.Invoke(null, new object[] { validationErrors })!;
            }
        }

        // Handle non-generic Result type
        return (TResponse)(object)Result.FailValidation(validationErrors);
    }
}
