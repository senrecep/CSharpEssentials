using CSharpEssentials.DependencyInjection.Benchmarks.Services.S50;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = [];
// https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration
ManualS50.Register(services);
// https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions.replace
ManualDecorationS50.Apply(services);

using ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();
int[] values = CatalogS50.Values(scope.ServiceProvider);
bool ok = values.Length == 50 && values.Select(static (v, i) => v == (i < 10 ? i + 1000 : i)).All(static b => b);
Console.WriteLine($"resolved {values.Length} checksum {values.Sum()} {(ok ? "ok" : "MISMATCH")}");
return ok ? 0 : 1;
