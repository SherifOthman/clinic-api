using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that automatically validates all requests using FluentValidation.
/// Runs before the handler executes.
/// Returns Result.Failure for validation errors instead of throwing exceptions.
/// </summary>
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
            // Check if TResponse is a Result type
            var responseType = typeof(TResponse);
            
            // Handle Result<T>
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var firstError = failures.First();
                var failureMethod = typeof(Result).GetMethod(nameof(Result.Failure))!
                    .MakeGenericMethod(responseType.GetGenericArguments()[0]);
                
                return (TResponse)failureMethod.Invoke(null, new object[] 
                { 
                    ErrorCodes.VALIDATION_ERROR, 
                    firstError.ErrorMessage 
                })!;
            }
            
            // Handle Result
            if (responseType == typeof(Result))
            {
                var firstError = failures.First();
                return (TResponse)(object)Result.Failure(ErrorCodes.VALIDATION_ERROR, firstError.ErrorMessage);
            }
            
            // For non-Result types, throw exception (fallback for backwards compatibility)
            throw new ValidationException(failures);
        }

        return await next();
    }
}
