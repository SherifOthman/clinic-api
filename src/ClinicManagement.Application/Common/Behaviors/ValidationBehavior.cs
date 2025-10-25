using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Extensions;
using ClinicManagement.Application.Utils;
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
        var failures =
            _validators.Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
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

        if (typeof(TResponse).IsGenericType)
        {
            var innerType = typeof(TResponse).GetGenericArguments()[0];
            var genericResultType = typeof(Result<>).MakeGenericType(innerType);
            var failureMethod = genericResultType.GetMethod("Fail", new[] { typeof(List<ErrorItem>) });
            return (TResponse)failureMethod!.Invoke(null, new object[] { errorList })!;
        }

        return (TResponse)(object)Result.Fail(errorList);
    }
}
