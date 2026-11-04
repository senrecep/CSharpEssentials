using CSharpEssentials.DependencyInjection.Benchmarks.Services.S50;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = [];
// https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning
// https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58
services.Scan(scan => scan.FromAssemblyOf<IService0>().AddClasses().AsMatchingInterface().WithScopedLifetime());
// https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#decoration
ServiceCollectionExtensions.Decorate<IService0, Service0Decorator>(services);
ServiceCollectionExtensions.Decorate<IService1, Service1Decorator>(services);
ServiceCollectionExtensions.Decorate<IService2, Service2Decorator>(services);
ServiceCollectionExtensions.Decorate<IService3, Service3Decorator>(services);
ServiceCollectionExtensions.Decorate<IService4, Service4Decorator>(services);
ServiceCollectionExtensions.Decorate<IService5, Service5Decorator>(services);
ServiceCollectionExtensions.Decorate<IService6, Service6Decorator>(services);
ServiceCollectionExtensions.Decorate<IService7, Service7Decorator>(services);
ServiceCollectionExtensions.Decorate<IService8, Service8Decorator>(services);
ServiceCollectionExtensions.Decorate<IService9, Service9Decorator>(services);

using ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();
int[] values = CatalogS50.Values(scope.ServiceProvider);
bool ok = values.Length == 50 && values.Select(static (v, i) => v == (i < 10 ? i + 1000 : i)).All(static b => b);
Console.WriteLine($"resolved {values.Length} checksum {values.Sum()} {(ok ? "ok" : "MISMATCH")}");
return ok ? 0 : 1;
