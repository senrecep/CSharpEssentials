using Npgsql;
using Testcontainers.PostgreSql;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>One PostgreSQL container for the enum storage tests; every test gets its own database.</summary>
public sealed class PostgresEnumFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string ConnectionString(string database) =>
        new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = database }.ConnectionString;

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
