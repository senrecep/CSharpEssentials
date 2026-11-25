# CSharpEssentials.EntityFrameworkCore.HealthChecks

A readiness health check for EF Core. It reports `Unhealthy` while a `DbContext` has migrations that are not applied, so `/health/ready` keeps traffic away from an instance whose database schema is behind its code.

It is a separate package because it depends on `Microsoft.Extensions.Diagnostics.HealthChecks`, which `CSharpEssentials.EntityFrameworkCore` does not.

## Install

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore.HealthChecks
```

Targets net10.0, net9.0 and net8.0, each on the EF Core major of `CSharpEssentials.EntityFrameworkCore`.

## Usage

```csharp
using CSharpEssentials.EntityFrameworkCore.HealthChecks;

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHealthChecks()
    .AddMigrationsCheck<AppDbContext>();   // name "AppDbContext-migrations", tag "ready"

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(MigrationsHealthCheckBuilderExtensions.ReadyTag),
});
```

| Member | What it does |
|--------|-------------|
| `AddMigrationsCheck<TContext>(name?, failureStatus?, tags?, timeout?)` | Adds the check. `name` defaults to `"{TContext}-migrations"`, `tags` to `["ready"]`, `failureStatus` to `Unhealthy` |
| `DbContextMigrationsHealthCheck<TContext>` | Calls `Database.GetPendingMigrationsAsync()` on a context from the health check scope. Pending migrations: the failure status, with the names under the `pendingMigrations` data key. All applied but `Database.HasPendingModelChanges()` is true (the model changed and no migration covers it): `Degraded`. Otherwise `Healthy` |
| `MigrationsHealthCheckBuilderExtensions.ReadyTag` | `"ready"` |

A database that does not exist yet has every migration pending. When the database cannot be reached, the query throws and the health check service reports the failure status with the exception. Either way the check fails, so it also covers connectivity.

To apply the migrations at startup instead of waiting for them, use `MigrateDatabaseAsync<TContext>()` from `CSharpEssentials.EntityFrameworkCore` (see [Migrate on startup](../CSharpEssentials.EntityFrameworkCore/Readme.MD#migrate-on-startup)).
