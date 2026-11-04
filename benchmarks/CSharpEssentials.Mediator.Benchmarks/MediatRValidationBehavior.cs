using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
// https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/IPipelineBehavior.cs
public sealed class MediatRValidationBehavior(IValidator<CreateOrder> validator) : IPipelineBehavior<CreateOrder, Result<int>>
{
    public async Task<Result<int>> Handle(CreateOrder request, RequestHandlerDelegate<Result<int>> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Result<CreateOrder> validation;
        try
        {
            validation = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            validation = Error.Exception("Validator.Exception", ex);
        }

        return validation.IsFailure
            ? Result<int>.Failure(validation.Errors)
            : await next(cancellationToken).ConfigureAwait(false);
    }
}
