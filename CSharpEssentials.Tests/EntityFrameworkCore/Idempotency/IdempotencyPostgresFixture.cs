using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Testcontainers.PostgreSql;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>One PostgreSQL database with the idempotency table, shared by the idempotency store tests.</summary>
public sealed class IdempotencyPostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = "idempotency" }.ConnectionString;
        await using IdempotencyTestDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public IdempotencyTestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<IdempotencyTestDbContext>().UseNpgsql(ConnectionString).Options);

    /// <summary>Services with <see cref="IdempotencyTestDbContext"/> on <paramref name="connectionString"/> (default: the fixture's database).</summary>
    public ServiceProvider CreateServices(Action<IServiceCollection>? configure = null, string? connectionString = null)
    {
        ServiceCollection services = new();
        services.AddDbContext<IdempotencyTestDbContext>(options => options.UseNpgsql(connectionString ?? ConnectionString));
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }
}

[CollectionDefinition(Name)]
public sealed class IdempotencyPostgresCollection : ICollectionFixture<IdempotencyPostgresFixture>
{
    public const string Name = "Idempotency PostgreSQL";
}
