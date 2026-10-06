using Application.Common.Dtos;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var failures = validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count == 0)
                return await next();

            var errorMessage = string.Join(" | ", failures.Select(f => f.ErrorMessage));

            var responseType = typeof(TResponse);
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = responseType.GetMethod(nameof(Result<object>.Failure), BindingFlags.Public | BindingFlags.Static);
                var result = failureMethod!.Invoke(null, [errorMessage]);
                return (TResponse)result!;
            }

            throw new ValidationException(failures);
        }
    }
}
