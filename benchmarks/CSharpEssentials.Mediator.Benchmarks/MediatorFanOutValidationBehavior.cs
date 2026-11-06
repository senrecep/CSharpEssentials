using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
// https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#331-message-validation-example
public sealed class MediatorFanOutValidationBehavior(IEnumerable<IValidator<CreateCustomer>> validators) : IPipelineBehavior<CreateCustomer, Result<int>>
{
    private readonly IValidator<CreateCustomer>[] _validators = [.. validators.OrderBy(static v => v.Order)];

    public async ValueTask<Result<int>> Handle(CreateCustomer message, MessageHandlerDelegate<CreateCustomer, Result<int>> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        List<Error>? errors = null;
        foreach (IValidator<CreateCustomer> validator in _validators)
        {
            Result<CreateCustomer> validation;
            try
            {
                validation = await validator.ValidateAsync(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                validation = Error.Exception("Validator.Exception", ex);
            }

            if (validation.IsFailure)
            {
                errors ??= [];
                errors.AddRange(validation.Errors);
            }
        }

        return errors is null
            ? await next(message, cancellationToken).ConfigureAwait(false)
            : Result<int>.Failure(errors);
    }
}
