using Microsoft.Data.Sqlite;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>The enum conversions against an in-memory SQLite database, which EF rebuilds for every column type change.</summary>
public sealed class EnumConversionSqliteTests : EnumConversionTests, IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public EnumConversionSqliteTests() => _connection.Open();

    protected override string Provider => "Microsoft.EntityFrameworkCore.Sqlite";

    public void Dispose() => _connection.Dispose();

    protected override StoredOrderContext Open(StoredOrderModel model) => StoredOrderContext.Sqlite(_connection, model);

    protected override async Task<string> ConstraintsAsync() =>
        string.Join("\n", await QueryAsync<string>($"SELECT sql AS \"Value\" FROM sqlite_master WHERE type = 'table' AND name = '{Table}'"));
}
