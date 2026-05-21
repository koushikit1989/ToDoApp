using FluentValidation;
using FluentValidation.Results;
using MediatR;
using DomainValidationException = ToDoManagementSystem.Domain.Exceptions.ValidationException;

namespace ToDoManagementSystem.Application.Behaviors;

/// <summary>MediatR pipeline behavior that runs FluentValidation before any handler.</summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>Executes all registered validators and throws on failure.</summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<string> failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => f.ErrorMessage)
            .ToList();

        if (failures.Count > 0)
            throw new DomainValidationException(failures);

        return await next();
    }
}
