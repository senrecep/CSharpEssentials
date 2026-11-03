using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/jbogard/MediatR/wiki/Behaviors
// https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs
public sealed class MediatRFanOutValidationBehavior(IEnumerable<IValidator<CreateCustomer>> validators) : IPipelineBehavior<CreateCustomer, Result<int>>
{
    private readonly IValidator<CreateCustomer>[] _validators = [.. validators.OrderBy(static v => v.Order)];

    public async Task<Result<int>> Handle(CreateCustomer request, RequestHandlerDelegate<Result<int>> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        List<Error>? errors = null;
        foreach (IValidator<CreateCustomer> validator in _validators)
        {
            Result<CreateCustomer> validation;
            try
            {
                validation = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
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
            ? await next(cancellationToken).ConfigureAwait(false)
            : Result<int>.Failure(errors);
    }
}
