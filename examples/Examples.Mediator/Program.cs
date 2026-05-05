using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Mediator Example");
Console.WriteLine("========================================\n");

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Information));

// ============================================================================
// 1. VALIDATION BEHAVIOR
// ============================================================================
Console.WriteLine("--- ValidationBehavior ---");

var validationBehavior = new ValidationBehavior<CreateProductCommand, Result<int>>(
    [new CreateProductCommandValidator()]);

Result<int> invalidResult = await validationBehavior.Handle(
    new CreateProductCommand("", -10),
    static (_, _) => new ValueTask<Result<int>>(Result.Success(1)),
    default);

Console.WriteLine($"Invalid command - IsFailure: {invalidResult.IsFailure}");
foreach (Error error in invalidResult.Errors)
    Console.WriteLine($"  [{error.Code}] {error.Description}");

Result<int> validResult = await validationBehavior.Handle(
    new CreateProductCommand("Widget", 99),
    static (_, _) => new ValueTask<Result<int>>(Result.Success(42)),
    default);

Console.WriteLine($"Valid command - IsSuccess: {validResult.IsSuccess}, ProductId: {validResult.Value}");

Console.WriteLine();

// ============================================================================
// 2. LOGGING BEHAVIOR
// ============================================================================
Console.WriteLine("--- LoggingBehavior ---");

// IRequestLoggable  → only request payload is logged
// IResponseLoggable → only response payload is logged
// IRequestResponseLoggable → both are logged

var requestLogger = loggerFactory.CreateLogger<LoggingBehavior<GetProductQuery, Result<string>>>();
var loggingBehavior = new LoggingBehavior<GetProductQuery, Result<string>>(requestLogger);

await loggingBehavior.Handle(
    new GetProductQuery("123"),
    static (q, _) => new ValueTask<Result<string>>(Result.Success<string>($"Product-{q.Id}")),
    default);

Console.WriteLine();

// ============================================================================
// 3. CACHING BEHAVIOR
// ============================================================================
Console.WriteLine("--- CachingBehavior ---");

var cacheLogger = loggerFactory.CreateLogger<CachingBehavior<GetProductQuery, Result<string>>>();
var cache = new DictionaryDistributedCache();
var cachingBehavior = new CachingBehavior<GetProductQuery, Result<string>>(cacheLogger, cache);
var query = new GetProductQuery("123");

int callCount = 0;

Result<string> firstCall = await cachingBehavior.Handle(
    query,
    (q, _) => { callCount++; return new ValueTask<Result<string>>(Result.Success<string>($"Product-{q.Id}")); },
    default);

Console.WriteLine($"First call (cache miss) : {firstCall.Value}, handler calls: {callCount}");

Result<string> secondCall = await cachingBehavior.Handle(
    query,
    (q, _) => { callCount++; return new ValueTask<Result<string>>(Result.Success<string>($"Product-{q.Id}")); },
    default);

Console.WriteLine($"Second call (cache hit) : {secondCall.Value}, handler calls: {callCount}");

Console.WriteLine();

// ============================================================================
// 4. TRANSACTION SCOPE BEHAVIOR
// ============================================================================
Console.WriteLine("--- TransactionScopeBehavior ---");

var transactionBehavior = new TransactionScopeBehavior<PlaceOrderCommand, Result>();

Result transactionResult = await transactionBehavior.Handle(
    new PlaceOrderCommand(Guid.NewGuid(), 250.00m),
    static (cmd, _) =>
    {
        Console.WriteLine($"  Executing inside transaction: order {cmd.OrderId} for ${cmd.Amount}");
        return new ValueTask<Result>(Result.Success());
    },
    default);

Console.WriteLine($"Transaction result: IsSuccess={transactionResult.IsSuccess}");

Console.WriteLine();

// ============================================================================
// 5. DI REGISTRATION
// ============================================================================
Console.WriteLine("--- DI Registration ---");

var services = new ServiceCollection();

// Register all behaviors in the default pipeline order:
// ValidationBehavior → LoggingBehavior → CachingBehavior → TransactionScopeBehavior
services.AddMediatorBehaviors();

// Or register them individually for custom ordering:
// services.AddMediatorValidationBehavior();
// services.AddMediatorLoggingBehavior();
// services.AddMediatorCachingBehavior();
// services.AddMediatorTransactionBehavior();

// For NativeAOT support, use MediatorOptions instead of open-generic DI registration:
// services.AddMediator(options =>
//     options.PipelineBehaviors = MediatorExtensions.DefaultPipelineBehaviors);

Console.WriteLine($"Registered behaviors: {services.Count}");

Console.WriteLine();
Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");

// ============================================================================
// TYPE DEFINITIONS
// ============================================================================

// IRequestLoggable  → request payload is logged by LoggingBehavior
public sealed record CreateProductCommand(string Name, decimal Price)
    : ICommand<Result<int>>, IRequestLoggable;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("NameRequired")
            .WithMessage("Name is required");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithErrorCode("PriceInvalid")
            .WithMessage("Price must be positive");
    }
}

// IResponseLoggable → response payload is logged by LoggingBehavior
// ICacheable        → result is cached by CachingBehavior
public sealed record GetProductQuery(string Id)
    : IQuery<Result<string>>, IResponseLoggable, ICacheable
{
    public string CacheKey => $"product:{Id}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    public bool BypassCache => false;
    public bool CacheFailures => false;
}

// ITransactionalRequest → handler runs inside a TransactionScope
public sealed record PlaceOrderCommand(Guid OrderId, decimal Amount)
    : ICommand<Result>, ITransactionalRequest;

// Minimal IDistributedCache backed by a dictionary — for demo purposes only
public sealed class DictionaryDistributedCache : IDistributedCache
{
    private readonly Dictionary<string, byte[]> _store = [];

    public byte[]? Get(string key) =>
        _store.TryGetValue(key, out byte[]? value) ? value : null;

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
        Task.FromResult(Get(key));

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
        _store[key] = value;

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);
        return Task.CompletedTask;
    }

    public void Refresh(string key) { }
    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;
    public void Remove(string key) => _store.Remove(key);
    public Task RemoveAsync(string key, CancellationToken token = default) { Remove(key); return Task.CompletedTask; }
}
