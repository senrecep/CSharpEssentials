using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;
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

    [Fact]
    public async Task TextToInteger_Should_KeepCheckConstraintsEnforced_When_ValueIsUnknown()
    {
        StoredOrderModel before = Model("member-name", Legacy(EnumStoredAs.MemberName));
        StoredOrderModel after = Model("int", Legacy(EnumStoredAs.Integer));
        await CreateAsync(before, "(1, 'Shipped', 0), (2, 'bogus', 0)");

        Func<Task> migrate = () => MigrateAsync(before, after, nameof(ConvertedOrder.Status), builder =>
            builder.ConvertEnumColumn<StoredOrderStatus>(Table, nameof(ConvertedOrder.Status), from: EnumStoredAs.Text, to: EnumStorage.Integer));

        await migrate.Should().ThrowAsync<SqliteException>();
        (await QueryAsync<long>("SELECT ignore_check_constraints AS \"Value\" FROM pragma_ignore_check_constraints")).Should().Equal(0L);
    }

    protected override async Task<string> ConstraintsAsync() =>
        string.Join("\n", await QueryAsync<string>($"SELECT sql AS \"Value\" FROM sqlite_master WHERE type = 'table' AND name = '{Table}'"));
}
