using CSharpEssentials.EntityFrameworkCore.HealthChecks;
using CSharpEssentials.EntityFrameworkCore.Migrations;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public sealed class MigratedItem
{
    public int Id { get; set; }
}

public sealed class MigratedDbContext(DbContextOptions<MigratedDbContext> options) : DbContext(options)
{
    public DbSet<MigratedItem> Items => Set<MigratedItem>();
}

[DbContext(typeof(MigratedDbContext))]
[Migration("20260101000000_CreateItems")]
public sealed class CreateItemsMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.CreateTable(
            name: "Items",
            columns: table => new { Id = table.Column<int>(nullable: false) },
            constraints: table => table.PrimaryKey("PK_Items", item => item.Id));
}

public sealed class MigrationsTests : IDisposable
{
    private sealed class RecordingLock(List<string> log) : IDatabaseMigrationLock
    {
        private sealed class Release(List<string> log) : IAsyncDisposable
        {
            public ValueTask DisposeAsync()
            {
                log.Add("released");
                return ValueTask.CompletedTask;
            }
        }

        public async ValueTask<IAsyncDisposable> AcquireAsync(DbContext context, CancellationToken cancellationToken)
        {
            log.Add($"acquired, pending {(await context.Database.GetPendingMigrationsAsync(cancellationToken)).Count()}");
            return new Release(log);
        }
    }

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public MigrationsTests() => _connection.Open();

    public void Dispose() => _connection.Dispose();

    private ServiceProvider BuildServices(Action<IServiceCollection>? configure = null)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddDbContext<MigratedDbContext>(options => options
            .UseSqlite(_connection)
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
        services.AddHealthChecks().AddMigrationsCheck<MigratedDbContext>();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Pending_Migrations_Report_Unhealthy_And_Applied_Ones_Healthy()
    {
        await using ServiceProvider provider = BuildServices();
        HealthCheckService health = provider.GetRequiredService<HealthCheckService>();

        HealthReport before = await health.CheckHealthAsync(check => check.Tags.Contains(MigrationsHealthCheckBuilderExtensions.ReadyTag));
        before.Status.Should().Be(HealthStatus.Unhealthy);
        HealthReportEntry entry = before.Entries["MigratedDbContext-migrations"];
        entry.Data["pendingMigrations"].Should().BeEquivalentTo(new[] { "20260101000000_CreateItems" });

        await provider.MigrateDatabaseAsync<MigratedDbContext>();

        HealthReport after = await health.CheckHealthAsync();
        after.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task Failure_Status_Can_Be_Lowered()
    {
        await using ServiceProvider provider = BuildServices(services =>
            services.AddHealthChecks().AddMigrationsCheck<MigratedDbContext>(name: "degraded", failureStatus: HealthStatus.Degraded));

        HealthReport report = await provider.GetRequiredService<HealthCheckService>()
            .CheckHealthAsync(check => check.Name == "degraded");

        report.Status.Should().Be(HealthStatus.Degraded);
    }

    [Fact]
    public async Task Missing_Database_Reports_The_Failure_Status()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddDbContext<MigratedDbContext>(options => options.UseSqlite("Data Source=/nonexistent-dir/db.sqlite;Mode=ReadOnly"));
        services.AddHealthChecks().AddMigrationsCheck<MigratedDbContext>();
        await using ServiceProvider provider = services.BuildServiceProvider();

        HealthReport report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        report.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task Registration_Uses_Default_Name_And_Ready_Tag()
    {
        await using ServiceProvider provider = BuildServices();

        HealthCheckRegistration registration = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value.Registrations.Single();

        registration.Name.Should().Be("MigratedDbContext-migrations");
        registration.Tags.Should().Equal("ready");
        registration.FailureStatus.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task MigrateDatabaseAsync_Runs_Inside_The_Registered_Lock()
    {
        List<string> log = [];
        await using ServiceProvider provider = BuildServices(services => services.AddSingleton<IDatabaseMigrationLock>(new RecordingLock(log)));

        await provider.MigrateDatabaseAsync<MigratedDbContext>();

        log.Should().Equal("acquired, pending 1", "released");
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        MigratedDbContext context = scope.ServiceProvider.GetRequiredService<MigratedDbContext>();
        (await context.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await context.Items.CountAsync()).Should().Be(0);
    }
}
