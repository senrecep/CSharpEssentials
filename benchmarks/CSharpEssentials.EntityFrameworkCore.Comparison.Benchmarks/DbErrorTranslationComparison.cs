using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.ResultPattern;
using Microsoft.Data.Sqlite;
#if NET10_0_OR_GREATER
using EntityFramework.Exceptions.Common;
using EntityFramework.Exceptions.Sqlite;
#endif

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// A save that violates a unique index on SQLite, detected three ways: catching the <c>SqliteException</c> by its extended
/// code, EntityFrameworkCore.Exceptions' processor (net10.0 and later; labelled: it throws a second, typed exception), and
/// CSE's <c>DbErrorTranslation.SaveChangesAsync</c>, which returns a failed <c>Result</c> instead of throwing (labelled:
/// SQLite reports no SQLSTATE, so CSE's documented SQLite path is a user-written translator, here
/// <see cref="SqliteUniqueTranslator"/>, copied from CSE's tests). The save fails, so the table never changes. Parity:
/// every row reports the unique violation and the table keeps one row.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the database and contexts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class DbErrorTranslationComparison
{
    private const string Duplicate = "dup";

    private BenchDatabase _database = null!;
    private PlainContext _handWritten = null!;
    private PlainContext _cse = null!;
    private ServiceProvider _services = null!;
    private DbErrorTranslation _translation = null!;
#if NET10_0_OR_GREATER
    private PlainContext _efExceptions = null!;
#endif

    [GlobalSetup]
    public void Setup()
    {
        _database = BenchDatabase.Create();
        using (PlainContext context = _database.Plain())
        {
            context.Set<UniqueItem>().Add(new UniqueItem { Name = Duplicate });
            context.SaveChanges();
        }

        _handWritten = _database.Plain();
        _cse = _database.Plain();
        _services = new ServiceCollection().AddDbErrorTranslator<SqliteUniqueTranslator>().BuildServiceProvider();
        _translation = _services.GetRequiredService<DbErrorTranslation>();
#if NET10_0_OR_GREATER
        _efExceptions = new PlainContext(_database.Options<PlainContext>(o => o.UseExceptionProcessor()));
#endif
        Verify().GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _handWritten.Dispose();
        _cse.Dispose();
#if NET10_0_OR_GREATER
        _efExceptions.Dispose();
#endif
        _services.Dispose();
        _database.Dispose();
    }

    // https://learn.microsoft.com/dotnet/api/microsoft.data.sqlite.sqliteexception.sqliteextendederrorcode
    // https://www.sqlite.org/rescode.html#constraint_unique
    [Benchmark(Baseline = true)]
    public async Task<bool> HandWritten_CatchSqliteException()
    {
        _handWritten.Set<UniqueItem>().Add(new UniqueItem { Name = Duplicate });
        try
        {
            await _handWritten.SaveChangesAsync();
            return false;
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            return true;
        }
        finally
        {
            _handWritten.ChangeTracker.Clear();
        }
    }

#if NET10_0_OR_GREATER
    // https://github.com/Giorgi/EntityFramework.Exceptions/blob/7f4e2d0f4336f2f1b16679f30b9866fdc1f558d0/README.md#how-do-i-get-started
    [Benchmark]
    public async Task<bool> EfExceptions_UseExceptionProcessor()
    {
        _efExceptions.Set<UniqueItem>().Add(new UniqueItem { Name = Duplicate });
        try
        {
            await _efExceptions.SaveChangesAsync();
            return false;
        }
        catch (UniqueConstraintException)
        {
            return true;
        }
        finally
        {
            _efExceptions.ChangeTracker.Clear();
        }
    }
#endif

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#database-error-translation
    [Benchmark]
    public async Task<Result<int>> CSharpEssentials_DbErrorTranslation()
    {
        _cse.Set<UniqueItem>().Add(new UniqueItem { Name = Duplicate });
        Result<int> saved = await _translation.SaveChangesAsync(_cse);
        _cse.ChangeTracker.Clear();
        return saved;
    }

    private async Task Verify()
    {
        Result<int> cse = await CSharpEssentials_DbErrorTranslation();
        Parity.Same("DbErrorTranslation",
            ("expected", "unique"),
            ("HandWritten", await HandWritten_CatchSqliteException() ? "unique" : "saved"),
#if NET10_0_OR_GREATER
            ("EntityFrameworkCore.Exceptions", await EfExceptions_UseExceptionProcessor() ? "unique" : "saved"),
#endif
            ("CSharpEssentials", cse.IsFailure && cse.FirstError.Code == "Sqlite.Unique" ? "unique" : "saved"));
        using PlainContext context = _database.Plain();
        Parity.True("DbErrorTranslation", await context.Set<UniqueItem>().CountAsync() == 1, "a duplicate row was written");
    }
}
