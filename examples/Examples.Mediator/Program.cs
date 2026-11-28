using CSharpEssentials.Errors;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Locking;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Transactions;
using CSharpEssentials.Validation.Extensions;
using Examples.Mediator;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

ServiceCollection services = new();
services.AddLogging(logging => logging.AddSimpleConsole(o => o.SingleLine = true).SetMinimumLevel(LogLevel.Information));
services.AddDistributedMemoryCache();

services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

services.AddMediatorBehaviors();
services.AddMediatorTransactionRunnerBehavior();
services.AddSingleton<ITransactionRunner, ConsoleTransactionRunner>();
services.AddMediatorLockBehavior();
services.AddMediatorIdempotencyBehavior(options =>
    options.ForRequest<CapturePayment>(request => request.Profile = "payments"));
services.AddIdempotencyExecutor(options =>
    options.AddProfile("payments", profile => profile.Retention = TimeSpan.FromDays(7)));

services.AddSingleton<Counters>();
services.AddValidator<CreateProduct, CreateProductValidator>();
services.AddValidator<ImportProduct, ImportProductValidator>();
services.TryAddEnumerable(ServiceDescriptor.Scoped<IValidationFailureObserver, ConsoleValidationObserver>());

await using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
await using AsyncServiceScope scope = provider.CreateAsyncScope();
ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
Counters counters = scope.ServiceProvider.GetRequiredService<Counters>();

Section("Validation (Enforce)");
Result<int> invalid = await sender.Send(new CreateProduct("", -1m));
Console.WriteLine($"Rejected before the handler: {Codes(invalid.Errors)}");
Result<int> created = await sender.Send(new CreateProduct("Widget", 9.99m));
Console.WriteLine($"Created product {created.Value}");

Section("Validation (LogOnly through IValidationModeOverride)");
Result imported = await sender.Send(new ImportProduct(""));
Console.WriteLine($"The observer saw the failure, the handler still ran: IsSuccess={imported.IsSuccess}");

Section("Exception handling");
Result failed = await sender.Send(new DeleteProduct(404));
Console.WriteLine($"Thrown exception became a failed Result: {failed.FirstError.Type} {failed.FirstError.Code}");

Section("Caching and logging");
await sender.Send(new GetProduct(1));
await sender.Send(new GetProduct(1));
Console.WriteLine($"Two queries, handler calls: {counters["GetProduct"]}");
await sender.Send(new GetProduct(1, BypassCache: true));
Console.WriteLine($"BypassCache skipped the cache, handler calls: {counters["GetProduct"]}");

Section("Transactions (ITransactionRunner)");
await sender.Send(new RenameProduct(1, "Gadget"));
await sender.Send(new RenameProduct(1, ""));

Section("Lock");
await Task.WhenAll(
    sender.Send(new RestockProduct(7, 10)).AsTask(),
    sender.Send(new RestockProduct(7, 5)).AsTask());
Console.WriteLine($"Two concurrent restocks of product 7, most at once: {counters.MaxConcurrent("Restock")}");
IResourceLock locks = scope.ServiceProvider.GetRequiredService<IResourceLock>();
await using (await locks.AcquireAsync("product:7", timeout: null))
{
    Result timedOut = await sender.Send(new RestockProduct(7, 1));
    Console.WriteLine($"Lock held elsewhere, LockTimeout passed: {timedOut.FirstError.Code}");
}

Section("Idempotency");
Result<Guid> first = await sender.Send(new CapturePayment("pay-1", "acme", 10m));
Result<Guid> retry = await sender.Send(new CapturePayment("pay-1", "acme", 10m));
Console.WriteLine($"Retry replayed the stored result: {first.Value == retry.Value}, handler calls: {counters["CapturePayment"]}");
Result<Guid> reused = await sender.Send(new CapturePayment("pay-1", "acme", 99m));
Console.WriteLine($"Same key, other amount: {reused.FirstError.Code}");
Result<Guid> otherTenant = await sender.Send(new CapturePayment("pay-1", "globex", 99m));
Console.WriteLine($"Same key, other tenant runs on its own: {otherTenant.IsSuccess}");
Result<Guid> missing = await sender.Send(new CapturePayment("", "acme", 10m));
Console.WriteLine($"No key: {missing.FirstError.Code}");
Result<Guid> unscoped = await sender.Send(new RefundPayment("refund-1"));
Console.WriteLine($"No scope and no caller: {unscoped.FirstError.Code}");
Result crash = await sender.Send(new ChargeCard("charge-1"));
Result crashRetry = await sender.Send(new ChargeCard("charge-1"));
Console.WriteLine($"Handler threw: {crash.FirstError.Code}, retry before the lease ends: {crashRetry.FirstError.Code}");

return 0;

static void Section(string title) => Console.WriteLine($"{Environment.NewLine}--- {title} ---");

static string Codes(IEnumerable<Error> errors) => string.Join(", ", errors.Select(error => error.Code));
