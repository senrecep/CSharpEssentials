using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.Errors;

using Microsoft.Data.Sqlite;

namespace CSharpEssentials.Tests.EntityFrameworkCore.DbErrors;

/// <summary>SQLite reports no SQLSTATE, so it needs a provider translator; this one maps SQLITE_CONSTRAINT_UNIQUE (2067).</summary>
public sealed class SqliteUniqueTranslator : IDbErrorTranslator
{
    public bool TryTranslate(Exception exception, out Error error)
    {
        if (exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            error = Error.Conflict("Sqlite.Unique");
            return true;
        }

        error = default;
        return false;
    }
}
