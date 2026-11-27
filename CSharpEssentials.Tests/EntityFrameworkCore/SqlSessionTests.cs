using System.Data;
using System.Data.Common;

using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Sql;
using CSharpEssentials.EntityFrameworkCore.Transactions;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public sealed class SqlSessionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<NotesDbContext> _options;

    public SqlSessionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<NotesDbContext>().UseSqlite(_connection).Options;

        using NotesDbContext context = new(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private static async Task InsertAsync(ISqlSession session, string text)
    {
        await using DbCommand command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = "INSERT INTO Notes (Text) VALUES ($text)";
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = "$text";
        parameter.Value = text;
        command.Parameters.Add(parameter);
        await command.ExecuteNonQueryAsync();
    }

    private List<string> StoredTexts()
    {
        using NotesDbContext context = new(_options);
        return [.. context.Notes.OrderBy(note => note.Id).Select(note => note.Text)];
    }

    [Fact]
    public void Session_Without_Transaction_Exposes_Context_Connection_And_No_Transaction()
    {
        using NotesDbContext context = new(_options);
        DbContextSqlSession<NotesDbContext> session = new(context);

        session.Connection.Should().BeSameAs(context.Database.GetDbConnection());
        session.Transaction.Should().BeNull();
        session.Create().Should().BeSameAs(session.Connection);
    }

    [Fact]
    public async Task Sql_Joins_Runner_Transaction_And_Commits_With_It()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        DbContextSqlSession<NotesDbContext> session = new(context);

        await runner.ExecuteAsync(
            async token =>
            {
                session.Transaction.Should().NotBeNull();
                session.Transaction!.Should().BeSameAs(context.Database.CurrentTransaction!.GetDbTransaction());
                await InsertAsync(session, "sql");
                context.Notes.Add(new TransactionNote { Text = "ef" });
                await context.SaveChangesAsync(token);
                return true;
            },
            _ => true);

        StoredTexts().Should().Equal("sql", "ef");
    }

    [Fact]
    public async Task Sql_Is_Rolled_Back_With_Runner_Transaction()
    {
        await using NotesDbContext context = new(_options);
        EfCoreTransactionRunner<NotesDbContext> runner = new(context);
        DbContextSqlSession<NotesDbContext> session = new(context);

        await runner.ExecuteAsync(
            async _ =>
            {
                await InsertAsync(session, "sql");
                return false;
            },
            committed => committed);

        StoredTexts().Should().BeEmpty();
    }

    [Fact]
    public async Task DbDataSource_Factory_Creates_A_New_Closed_Connection_Each_Call()
    {
        string connectionString = $"Data Source=sqlsession-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using SqliteConnection keeper = new(connectionString);
        await keeper.OpenAsync();
        await using (SqliteCommand create = keeper.CreateCommand())
        {
            create.CommandText = "CREATE TABLE Items (Name TEXT); INSERT INTO Items VALUES ('a');";
            await create.ExecuteNonQueryAsync();
        }

        await using DbDataSource dataSource = SqliteFactory.Instance.CreateDataSource(connectionString);
        DbDataSourceSqlConnectionFactory factory = new(dataSource);

        using IDbConnection first = factory.Create();
        using IDbConnection second = factory.Create();
        first.Should().NotBeSameAs(second);
        first.State.Should().Be(ConnectionState.Closed);

        first.Open();
        using IDbCommand query = first.CreateCommand();
        query.CommandText = "SELECT Name FROM Items";
        query.ExecuteScalar().Should().Be("a");
    }

    [Fact]
    public void Registration_Resolves_Session_Factory_And_ReadOnly_Factory()
    {
        ServiceCollection services = new();
        services.AddSingleton<DbDataSource>(SqliteFactory.Instance.CreateDataSource("Data Source=:memory:"));
        services.AddScoped(_ => new NotesDbContext(_options));
        services.AddDbContextSqlSession<NotesDbContext>();
        services.AddDbDataSourceReadOnlyConnectionFactory();

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        NotesDbContext context = scope.ServiceProvider.GetRequiredService<NotesDbContext>();

        scope.ServiceProvider.GetRequiredService<ISqlSession>().Connection.Should().BeSameAs(context.Database.GetDbConnection());
        scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>().Should().BeOfType<DbContextSqlSession<NotesDbContext>>();
        scope.ServiceProvider.GetRequiredService<ISqlReadOnlyConnectionFactory>().Should().BeOfType<DbDataSourceSqlConnectionFactory>();
    }
}
