using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.Errors;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.DbErrors;

public sealed class SqlStateErrorTranslatorTests
{
    private readonly SqlStateErrorTranslator _translator = new();

    [Theory]
    [InlineData("23505", ErrorType.Conflict, "Database.UniqueViolation", false)]
    [InlineData("23503", ErrorType.Conflict, "Database.ForeignKeyViolation", false)]
    [InlineData("23514", ErrorType.Validation, "Database.CheckViolation", false)]
    [InlineData("23502", ErrorType.Validation, "Database.NotNullViolation", false)]
    [InlineData("40001", ErrorType.Conflict, "Database.SerializationFailure", true)]
    [InlineData("40P01", ErrorType.Conflict, "Database.Deadlock", true)]
    public void TryTranslate_Should_Map_Known_SqlState(string sqlState, ErrorType type, string code, bool retryable)
    {
        bool translated = _translator.TryTranslate(new FakeDbException(sqlState), out Error error);

        translated.Should().BeTrue();
        error.Type.Should().Be(type);
        error.Code.Should().Be(code);
        error.Description.Should().NotContain("secret");
        error.Metadata.Should().NotBeNull();
        error.Metadata[SqlStateErrorTranslator.SqlStateKey].Should().Be(sqlState);
        error.Metadata.ContainsKey(SqlStateErrorTranslator.RetryableKey).Should().Be(retryable);
        error.Metadata.Should().NotContainKey(SqlStateErrorTranslator.EntitiesKey);
        if (retryable)
            error.Metadata[SqlStateErrorTranslator.RetryableKey].Should().Be(true);
    }

    [Theory]
    [InlineData("42P01")]
    [InlineData("")]
    [InlineData(null)]
    public void TryTranslate_Should_Return_False_For_Unknown_Or_Missing_SqlState(string? sqlState)
    {
        _translator.TryTranslate(new FakeDbException(sqlState), out Error error).Should().BeFalse();
        error.Should().Be(default(Error));
    }

    [Fact]
    public void TryTranslate_Should_Return_False_For_Non_Database_Exception()
    {
        _translator.TryTranslate(new InvalidOperationException("boom"), out _).Should().BeFalse();
    }

    [Fact]
    public void TryTranslate_Should_Find_SqlState_In_Inner_Exceptions()
    {
        Exception exception = new InvalidOperationException("outer", new DbUpdateException("save failed", new FakeDbException("23505")));

        _translator.TryTranslate(exception, out Error error).Should().BeTrue();
        error.Code.Should().Be("Database.UniqueViolation");
    }

    [Fact]
    public void TryTranslate_Should_Add_Entity_Types_From_DbUpdateException()
    {
        using SqliteConnection connection = new("DataSource=:memory:");
        using TranslatorDbContext context = new(new DbContextOptionsBuilder<TranslatorDbContext>().UseSqlite(connection).Options);
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry first = context.Add(new TranslatorItem { Name = "a" });
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry second = context.Add(new TranslatorItem { Name = "b" });
        DbUpdateException exception = new("save failed", new FakeDbException("23505"), [first, second]);

        _translator.TryTranslate(exception, out Error error).Should().BeTrue();

        error.Metadata![SqlStateErrorTranslator.EntitiesKey].Should().BeEquivalentTo(new[] { nameof(TranslatorItem) });
    }

    [Fact]
    public void TryTranslate_Should_Add_Entity_Types_From_Wrapped_DbUpdateException()
    {
        using SqliteConnection connection = new("DataSource=:memory:");
        using TranslatorDbContext context = new(new DbContextOptionsBuilder<TranslatorDbContext>().UseSqlite(connection).Options);
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry = context.Add(new TranslatorItem { Name = "a" });
        Exception exception = new InvalidOperationException("retry limit exceeded", new DbUpdateException("save failed", new FakeDbException("40001"), [entry]));

        _translator.TryTranslate(exception, out Error error).Should().BeTrue();

        error.Metadata![SqlStateErrorTranslator.EntitiesKey].Should().BeEquivalentTo(new[] { nameof(TranslatorItem) });
    }

    [Fact]
    public void TryTranslate_Should_Create_New_Metadata_For_Every_Error()
    {
        _translator.TryTranslate(new FakeDbException("23505"), out Error first);
        _translator.TryTranslate(new FakeDbException("23505"), out Error second);

        first.Metadata!.AddMetadata("constraint", "ix_users_email");

        second.Metadata.Should().NotBeSameAs(first.Metadata);
        second.Metadata.Should().NotContainKey("constraint");
    }

    [Fact]
    public void TryTranslate_Should_Throw_For_Null_Exception()
    {
        Action act = () => _translator.TryTranslate(null!, out _);

        act.Should().Throw<ArgumentNullException>();
    }
}
