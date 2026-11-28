# CSharpEssentials.EntityFrameworkCore.HealthChecks

A readiness health check for EF Core. It fails while a `DbContext` has migrations that are not applied, so a readiness endpoint keeps traffic away from an instance whose database schema is behind its code.

It is a separate package because it depends on `Microsoft.Extensions.Diagnostics.HealthChecks`, which `CSharpEssentials.EntityFrameworkCore` does not.

## Install

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore.HealthChecks
```

Targets `net10.0`, `net9.0` and `net8.0`, each with the EF Core major of `CSharpEssentials.EntityFrameworkCore`.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore.HealthChecks
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.EntityFrameworkCore.HealthChecks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();

ServiceCollection services = new();
services.AddLogging();
services.AddDbContext<AppDb>(options => options.UseSqlite(connection));
services.AddHealthChecks().AddMigrationsCheck<AppDb>();

await using ServiceProvider provider = services.BuildServiceProvider();
HealthReport report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

foreach ((string name, HealthReportEntry entry) in report.Entries)
    Console.WriteLine($"{name} [{string.Join(",", entry.Tags)}]: {entry.Status}. {entry.Description}");
// AppDb-migrations [ready]: Degraded. AppDb has model changes that no migration covers.
// This sample has no migrations at all. With pending migrations the status is Unhealthy, with all applied Healthy.

public sealed class Item
{
    public int Id { get; set; }
}

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();
}
```

## In a web app

Map the check on a readiness endpoint that filters on the `ready` tag:

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore.HealthChecks
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.EntityFrameworkCore.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(options => options.UseSqlite("Data Source=app.db"));
builder.Services.AddHealthChecks().AddMigrationsCheck<AppDb>();   // name "AppDb-migrations", tag "ready"

WebApplication app = builder.Build();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(MigrationsHealthCheckBuilderExtensions.ReadyTag),
});
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.Run();

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options);
```

## API

`AddMigrationsCheck<TContext>(string? name = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)` on `IHealthChecksBuilder` adds `DbContextMigrationsHealthCheck<TContext>`.

| Parameter | Default |
|---|---|
| `name` | `"{TContext}-migrations"`, for example `"AppDb-migrations"` |
| `failureStatus` | `Unhealthy` |
| `tags` | `["ready"]` (`MigrationsHealthCheckBuilderExtensions.ReadyTag`) |
| `timeout` | none |

The check resolves `TContext` from the health check scope, so register the context first.

## Results

| Database state | Status | Details |
|---|---|---|
| Migrations not applied (`Database.GetPendingMigrationsAsync()` returns names) | the failure status | the names are under the data key `pendingMigrations` |
| All migrations applied, but `Database.HasPendingModelChanges()` is true | `Degraded` | the model changed and no migration covers it |
| All migrations applied and the model matches | `Healthy` | |
| Database does not exist | the failure status | every migration counts as pending |
| Database cannot be reached | the failure status | the query throws and the health check service reports the exception |

Because an unreachable database fails the check too, it also covers connectivity. A context that never had migrations (schema made with `EnsureCreated`) reports `Degraded`.

To apply migrations at startup instead of waiting for them, call `MigrateDatabaseAsync<TContext>()` from `CSharpEssentials.EntityFrameworkCore` before the app starts serving.

## Links

- Agent skill: [csharpessentials-efcore](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-efcore/SKILL.md)
- Example project: [examples/Examples.EntityFrameworkCore.Operations](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.EntityFrameworkCore.Operations)
- Core package and `MigrateDatabaseAsync`: [CSharpEssentials.EntityFrameworkCore](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#migrate-on-startup)
