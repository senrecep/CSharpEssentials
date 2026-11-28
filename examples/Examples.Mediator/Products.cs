using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using Mediator;

namespace Examples.Mediator;

internal sealed record CreateProduct(string? Name, decimal Price) : ICommand<Result<int>>, IRequestLoggable;

internal sealed class CreateProductValidator : Validator<CreateProduct>
{
    protected override ValueTask Configure(CreateProduct model, RuleContext<CreateProduct> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).NotEmpty();
        rules.For(() => model.Price).GreaterThan(0m);
        return ValueTask.CompletedTask;
    }
}

internal sealed class CreateProductHandler : ICommandHandler<CreateProduct, Result<int>>
{
    public ValueTask<Result<int>> Handle(CreateProduct command, CancellationToken cancellationToken) =>
        new(Result<int>.Success(1));
}

internal sealed record ImportProduct(string? Name) : ICommand<Result>, IValidationModeOverride
{
    public ValidationMode ValidationMode => ValidationMode.LogOnly;
}

internal sealed class ImportProductValidator : Validator<ImportProduct>
{
    protected override ValueTask Configure(ImportProduct model, RuleContext<ImportProduct> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).NotEmpty();
        return ValueTask.CompletedTask;
    }
}

internal sealed class ImportProductHandler : ICommandHandler<ImportProduct, Result>
{
    public ValueTask<Result> Handle(ImportProduct command, CancellationToken cancellationToken) => new(Result.Success());
}

internal sealed class ConsoleValidationObserver : IValidationFailureObserver
{
    public ValueTask OnValidationFailedAsync(ValidationFailureContext failure, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Observer: {failure.RequestType.Name} failed in {failure.Mode} mode: {string.Join(", ", failure.Errors.Select(e => e.Code))}");
        return ValueTask.CompletedTask;
    }
}

internal sealed record DeleteProduct(int Id) : ICommand<Result>;

internal sealed class DeleteProductHandler : ICommandHandler<DeleteProduct, Result>
{
    public ValueTask<Result> Handle(DeleteProduct command, CancellationToken cancellationToken) =>
        throw new InvalidOperationException($"Product {command.Id} does not exist.");
}

internal sealed record GetProduct(int Id, bool BypassCache = false) : IQuery<Result<string>>, ICacheable, IResponseLoggable
{
    public string CacheKey => $"product:{Id}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    public bool CacheFailures => false;
}

internal sealed class GetProductHandler(Counters counters) : IQueryHandler<GetProduct, Result<string>>
{
    public ValueTask<Result<string>> Handle(GetProduct query, CancellationToken cancellationToken)
    {
        counters.Increment("GetProduct");
        return new(Result<string>.Success($"Product {query.Id}"));
    }
}

internal sealed record RenameProduct(int Id, string Name) : ICommand<Result>, ITransactionalRequest;

internal sealed class RenameProductHandler : ICommandHandler<RenameProduct, Result>
{
    public ValueTask<Result> Handle(RenameProduct command, CancellationToken cancellationToken) =>
        new(command.Name.Length == 0
            ? Result.Failure(CSharpEssentials.Errors.Error.Validation("Product.NameEmpty", "The name is empty."))
            : Result.Success());
}

internal sealed record RestockProduct(int Id, int Quantity) : ICommand<Result>, ILockedRequest
{
    public string LockKey => $"product:{Id}";
    public TimeSpan? LockTimeout => TimeSpan.FromMilliseconds(200);
}

internal sealed class RestockProductHandler(Counters counters) : ICommandHandler<RestockProduct, Result>
{
    public async ValueTask<Result> Handle(RestockProduct command, CancellationToken cancellationToken)
    {
        counters.Enter("Restock");
        await Task.Delay(50, cancellationToken);
        counters.Leave("Restock");
        return Result.Success();
    }
}
