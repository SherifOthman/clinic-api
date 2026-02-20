using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            var validationErrors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var responseType = typeof(TResponse);
            
            // Handle Result<T>
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = typeof(Result).GetMethod(nameof(Result.ValidationFailure))!
                    .MakeGenericMethod(responseType.GetGenericArguments()[0]);
                
                return (TResponse)failureMethod.Invoke(null, new object[] 
                { 
                    ErrorCodes.VALIDATION_ERROR, 
                    "One or more validation errors occurred",
                    validationErrors
                })!;
            }
            
            // Handle Result
            if (responseType == typeof(Result))
            {
                return (TResponse)(object)Result.ValidationFailure(
                    ErrorCodes.VALIDATION_ERROR, 
                    "One or more validation errors occurred",
                    validationErrors);
            }
            
            // For non-Result types, throw exception (fallback for backwards compatibility)
            throw new ValidationException(failures);
        }

        return await next();
    }
}
