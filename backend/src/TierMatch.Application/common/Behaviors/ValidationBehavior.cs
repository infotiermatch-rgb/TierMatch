using FluentValidation;
using MediatR;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators)
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
            _validators.Select(v =>
                v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var message = string.Join(
                Environment.NewLine,
                failures.Select(f => f.ErrorMessage));

            object result;

            if (typeof(TResponse) == typeof(Result))
            {
                result = Result.Validation(message);
            }
            else if (typeof(TResponse).IsGenericType &&
                     typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var valueType = typeof(TResponse).GetGenericArguments()[0];

                var validationMethod = typeof(Result<>)
                    .MakeGenericType(valueType)
                    .GetMethod(nameof(Result<object>.Validation));

                result = validationMethod!.Invoke(
                    null,
                    new object[] { message })!;
            }
            else
            {
                throw new ValidationException(failures);
            }

            return (TResponse)result;
        }

        return await next();
    }
}