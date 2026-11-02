using CSharpEssentials.DependencyInjection.Benchmarks.Services.S50;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = [];
services.AddServicesFromAssemblies(typeof(IService0).Assembly, typeof(Service0Decorator).Assembly);

using ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();
int[] values = CatalogS50.Values(scope.ServiceProvider);
bool ok = values.Length == 50 && values.Select(static (v, i) => v == (i < 10 ? i + 1000 : i)).All(static b => b);
Console.WriteLine($"resolved {values.Length} checksum {values.Sum()} {(ok ? "ok" : "MISMATCH")}");
return ok ? 0 : 1;
