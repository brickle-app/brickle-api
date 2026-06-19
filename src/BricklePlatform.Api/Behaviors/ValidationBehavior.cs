using FluentValidation;
using MediatR;

namespace BricklePlatform.Api.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validando request de tipo: {RequestType}", typeof(TRequest).Name);
        _logger.LogInformation("Validadores encontrados: {ValidatorCount}", _validators.Count());

        if (_validators.Any())
        {
            ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);
            FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            List<FluentValidation.Results.ValidationFailure> failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
            {
                _logger.LogWarning("Validación falló con {FailureCount} errores", failures.Count);

                foreach (FluentValidation.Results.ValidationFailure? failure in failures)
                {
                    _logger.LogWarning("Error de validación: {PropertyName} - {ErrorMessage}", failure.PropertyName, failure.ErrorMessage);
                }

                throw new FluentValidation.ValidationException(failures);
            }
        }
        else
        {
            _logger.LogWarning("No se encontraron validadores para el tipo: {RequestType}", typeof(TRequest).Name);
        }

        return await next();
    }
}