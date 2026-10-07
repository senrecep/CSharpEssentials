using CSharpEssentials.AspNetCore;
using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.DbErrors;

/// <summary>Translation of real PostgreSQL errors, which carry a SQLSTATE through <c>DbException.SqlState</c>.</summary>
public sealed class DbErrorTranslationPostgresTests(PostgresEnumFixture postgres) : IClassFixture<PostgresEnumFixture>
{
    private readonly DbErrorTranslation _translation = new([]);

    [Fact]
    public async Task SaveChangesAsync_Should_Return_Conflict_With_Entities_On_Unique_Violation()
    {
        await using TranslatorDbContext context = await CreateAsync("db_errors_unique");
        context.Items.Add(new TranslatorItem { Name = "taken" });
        (await _translation.SaveChangesAsync(context)).IsSuccess.Should().BeTrue();
        context.ChangeTracker.Clear();
        context.Items.Add(new TranslatorItem { Name = "taken" });

        Result<int> result = await _translation.SaveChangesAsync(context);

        result.IsFailure.Should().BeTrue();
        Error error = result.FirstError;
        error.Type.Should().Be(ErrorType.Conflict);
        error.Code.Should().Be("Database.UniqueViolation");
        error.Metadata![SqlStateErrorTranslator.SqlStateKey].Should().Be(SqlStateErrorTranslator.UniqueViolation);
        error.Metadata[SqlStateErrorTranslator.EntitiesKey].Should().BeEquivalentTo(new[] { nameof(TranslatorItem) });
        result.ToProblemDetails().Status.Should().Be(409);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Return_Validation_On_Not_Null_Violation()
    {
        await using TranslatorDbContext context = await CreateAsync("db_errors_not_null");
        context.Items.Add(new TranslatorItem { Name = null! });

        Result<int> result = await _translation.SaveChangesAsync(context);

        result.FirstError.Code.Should().Be("Database.NotNullViolation");
        result.ToProblemDetails().Status.Should().Be(400);
    }

    private async Task<TranslatorDbContext> CreateAsync(string database)
    {
        TranslatorDbContext context = new(new DbContextOptionsBuilder<TranslatorDbContext>()
            .UseNpgsql(postgres.ConnectionString(database))
            .Options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }
}
