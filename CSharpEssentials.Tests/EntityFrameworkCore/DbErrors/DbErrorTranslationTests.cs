using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore.DbErrors;

public sealed class DbErrorTranslationTests
{
    [Fact]
    public void TryTranslate_Should_Use_SqlState_Default_When_No_Translator_Is_Registered()
    {
        DbErrorTranslation translation = new([]);

        translation.TryTranslate(new FakeDbException("23505"), out Error error).Should().BeTrue();
        error.Code.Should().Be("Database.UniqueViolation");
    }

    [Fact]
    public void TryTranslate_Should_Try_Registered_Translators_In_Order_Before_Default()
    {
        RecordingTranslator skipping = new(null);
        RecordingTranslator first = new(Error.Conflict("First"));
        RecordingTranslator second = new(Error.Conflict("Second"));
        DbErrorTranslation translation = new([skipping, first, second]);

        translation.TryTranslate(new FakeDbException("23505"), out Error error).Should().BeTrue();

        error.Code.Should().Be("First");
        skipping.Calls.Should().Be(1);
        first.Calls.Should().Be(1);
        second.Calls.Should().Be(0);
    }

    [Fact]
    public void TryTranslate_Should_Fall_Back_To_Default_When_Custom_Translators_Do_Not_Match()
    {
        DbErrorTranslation translation = new([new RecordingTranslator(null)]);

        translation.TryTranslate(new FakeDbException("40P01"), out Error error).Should().BeTrue();
        error.Code.Should().Be("Database.Deadlock");
    }

    [Fact]
    public void TryTranslate_Should_Return_False_When_Nothing_Matches()
    {
        DbErrorTranslation translation = new([new RecordingTranslator(null)]);

        translation.TryTranslate(new InvalidOperationException("boom"), out Error error).Should().BeFalse();
        error.Should().Be(default(Error));
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Return_Written_Count_On_Success()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using TranslatorDbContext context = await CreateAsync(connection);
        context.Items.Add(new TranslatorItem { Name = "a" });

        Result<int> result = await new DbErrorTranslation([]).SaveChangesAsync(context);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Return_Translated_Error_When_A_Translator_Matches()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using TranslatorDbContext context = await CreateAsync(connection);
        context.Items.AddRange(new TranslatorItem { Name = "a" }, new TranslatorItem { Name = "a" });
        DbErrorTranslation translation = new([new SqliteUniqueTranslator()]);

        Result<int> result = await translation.SaveChangesAsync(context);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Sqlite.Unique");
        context.ChangeTracker.HasChanges().Should().BeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Rethrow_Unrecognized_Exceptions()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using TranslatorDbContext context = await CreateAsync(connection);
        context.Items.AddRange(new TranslatorItem { Name = "a" }, new TranslatorItem { Name = "a" });

        Func<Task> act = async () => await new DbErrorTranslation([]).SaveChangesAsync(context);

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public void AddDbErrorTranslator_Should_Register_Translators_In_Order_Once()
    {
        ServiceCollection services = new();
        services.AddDbErrorTranslator<SqliteUniqueTranslator>();
        services.AddDbErrorTranslator<NeverTranslator>();
        services.AddDbErrorTranslator<SqliteUniqueTranslator>();
        services.AddDbErrorTranslation();

        services.Where(d => d.ServiceType == typeof(IDbErrorTranslator)).Select(d => d.ImplementationType)
            .Should().Equal(typeof(SqliteUniqueTranslator), typeof(NeverTranslator));
        services.Should().ContainSingle(d => d.ServiceType == typeof(DbErrorTranslation) && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Rethrow_Cancellation()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using TranslatorDbContext context = await CreateAsync(connection);
        context.Items.Add(new TranslatorItem { Name = "a" });

        Func<Task> act = async () => await new DbErrorTranslation([]).SaveChangesAsync(context, new CancellationToken(canceled: true));

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void AddDbErrorTranslator_Should_Resolve_Translators_In_Registration_Order()
    {
        ServiceCollection services = new();
        services.AddDbErrorTranslator<NeverTranslator>();
        services.AddDbErrorTranslator<SqliteUniqueTranslator>();
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetServices<IDbErrorTranslator>().Select(t => t.GetType())
            .Should().Equal(typeof(NeverTranslator), typeof(SqliteUniqueTranslator));
    }

    [Fact]
    public void AddDbErrorTranslation_Should_Resolve_With_Default_Only()
    {
        using ServiceProvider provider = new ServiceCollection().AddDbErrorTranslation().BuildServiceProvider();

        provider.GetRequiredService<DbErrorTranslation>()
            .TryTranslate(new FakeDbException("23503"), out Error error).Should().BeTrue();
        error.Code.Should().Be("Database.ForeignKeyViolation");
    }

    private static async Task<SqliteConnection> OpenAsync()
    {
        SqliteConnection connection = new("DataSource=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<TranslatorDbContext> CreateAsync(SqliteConnection connection)
    {
        TranslatorDbContext context = new(new DbContextOptionsBuilder<TranslatorDbContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }
}
