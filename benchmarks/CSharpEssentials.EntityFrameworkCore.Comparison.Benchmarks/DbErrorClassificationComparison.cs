using System.Data.Common;
using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.Errors;
using Microsoft.Data.Sqlite;
#if NET10_0_OR_GREATER
using DbExceptionClassifier.Sqlite;
#endif

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// Classifying an exception that was already caught, without a database. Sqlite: a <c>DbUpdateException</c> wrapping
/// <c>SqliteException(19, 2067)</c>, read by a pattern match, DbExceptionClassifier (net10.0 and later; it takes the inner
/// <c>DbException</c>) and CSE's <c>DbErrorTranslation</c> with the SQLite translator, which also builds an <c>Error</c>.
/// SqlState: a <c>DbException</c> reporting SQLSTATE 23505, read by a pattern match and CSE's <c>SqlStateErrorTranslator</c>
/// (labelled: it builds metadata and an <c>Error</c>); no competitor takes a SQLSTATE-only exception. Parity: every row
/// classifies the input as a unique violation.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class DbErrorClassificationComparison
{
    private const string Sqlite = "Sqlite";
    private const string SqlState = "SqlState";

    private readonly DbException _sqliteInner = new SqliteException("SQLite Error 19: 'UNIQUE constraint failed: unique_items.Name'.", 19, 2067);
    private readonly DbException _sqlState = new FakeDbException(SqlStateErrorTranslator.UniqueViolation);
    private readonly DbErrorTranslation _translation = new([new SqliteUniqueTranslator()]);
    private readonly SqlStateErrorTranslator _sqlStateTranslator = new();
#if NET10_0_OR_GREATER
    private readonly SqliteExceptionClassifier _classifier = new();
#endif
    private DbUpdateException _sqlite = null!;

    [GlobalSetup]
    public void Setup()
    {
        _sqlite = new DbUpdateException("An error occurred while saving the entity changes.", _sqliteInner);
        Parity.Same("DbErrorClassification",
            ("expected", bool.TrueString),
            ("HandWritten Sqlite", HandWritten_PatternMatch().ToString()),
#if NET10_0_OR_GREATER
            ("DbExceptionClassifier", DbExceptionClassifier_IsUniqueConstraintError().ToString()),
#endif
            ("CSharpEssentials Sqlite", CSharpEssentials_DbErrorTranslation_SqliteTranslator().ToString()),
            ("HandWritten SqlState", HandWritten_SqlStateSwitch().ToString()),
            ("CSharpEssentials SqlState", CSharpEssentials_SqlStateErrorTranslator().ToString()));
    }

    // https://learn.microsoft.com/dotnet/api/microsoft.data.sqlite.sqliteexception.sqliteextendederrorcode
    // https://www.sqlite.org/rescode.html#constraint_unique
    [Benchmark(Baseline = true), BenchmarkCategory(Sqlite)]
    public bool HandWritten_PatternMatch() => _sqlite.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 };

#if NET10_0_OR_GREATER
    // https://github.com/Giorgi/EntityFramework.Exceptions/blob/7f4e2d0f4336f2f1b16679f30b9866fdc1f558d0/README.md#dbexceptionclassifier
    [Benchmark, BenchmarkCategory(Sqlite)]
    public bool DbExceptionClassifier_IsUniqueConstraintError() => _classifier.IsUniqueConstraintError(_sqliteInner);
#endif

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#database-error-translation
    [Benchmark, BenchmarkCategory(Sqlite)]
    public bool CSharpEssentials_DbErrorTranslation_SqliteTranslator() => _translation.TryTranslate(_sqlite, out Error error) && error.Type == ErrorType.Conflict;

    // https://www.postgresql.org/docs/current/errcodes-appendix.html
    [Benchmark(Baseline = true), BenchmarkCategory(SqlState)]
    public bool HandWritten_SqlStateSwitch() => _sqlState is { SqlState: SqlStateErrorTranslator.UniqueViolation };

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#database-error-translation
    [Benchmark, BenchmarkCategory(SqlState)]
    public bool CSharpEssentials_SqlStateErrorTranslator() => _sqlStateTranslator.TryTranslate(_sqlState, out Error error) && error.Type == ErrorType.Conflict;
}
