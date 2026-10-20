using Examples.Idempotency.EndToEnd.App;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Testcontainers.PostgreSql;

namespace Examples.Idempotency.EndToEnd;

/// <summary>
/// One PostgreSQL container with the payments schema, shared by every scenario; each scenario uses its own merchant
/// and provider account, so the scenarios never see each other's rows.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = "payments" }.ConnectionString;
        await using PaymentsDbContext db = CreateContext();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public PaymentsDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<PaymentsDbContext>().UseNpgsql(ConnectionString).Options);
}
