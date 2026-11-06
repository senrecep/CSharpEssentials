using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
public sealed class MediatorValidationBehavior(IValidator<CreateOrder> validator) : IPipelineBehavior<CreateOrder, Result<int>>
{
    public async ValueTask<Result<int>> Handle(CreateOrder message, MessageHandlerDelegate<CreateOrder, Result<int>> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Result<CreateOrder> validation;
        try
        {
            validation = await validator.ValidateAsync(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            validation = Error.Exception("Validator.Exception", ex);
        }

        return validation.IsFailure
            ? Result<int>.Failure(validation.Errors)
            : await next(message, cancellationToken).ConfigureAwait(false);
    }
}
