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

        if (IsResultType(typeof(TResponse)))
            return CreateFailureResult(failures);

        throw new ValidationException(failures);
    }

    private static bool IsResultType(Type type) =>
        typeof(Result).IsAssignableFrom(type) ||
        (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>));

    private static TResponse CreateFailureResult(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
    {
        var errorList = failures.ToErrorItemList();

        // Handle Result<T> types using reflection (appropriate for generic pipeline behavior)
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            // For Result<T>, we need to call Result<T>.Fail(errorList)
            var resultType = typeof(TResponse);
            var failMethod = resultType.GetMethod("Fail", new[] { typeof(IEnumerable<ErrorItem>) });
            
            if (failMethod != null)
            {
                return (TResponse)failMethod.Invoke(null, new object[] { errorList })!;
            }
        }

        // Handle non-generic Result type
        return (TResponse)(object)Result.Fail(errorList);
    }
}
