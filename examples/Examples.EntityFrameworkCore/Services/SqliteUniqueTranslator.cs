using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.Errors;
using Microsoft.Data.Sqlite;

namespace Examples.EntityFrameworkCore.Services;

public sealed class SqliteUniqueTranslator : IDbErrorTranslator
{
    private const int SqliteConstraintUnique = 2067;

    public bool TryTranslate(Exception exception, out Error error)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqliteException { SqliteExtendedErrorCode: SqliteConstraintUnique })
            {
                error = Error.Conflict("Database.UniqueViolation", "A record with the same unique value already exists.");
                return true;
            }
        }

        error = default;
        return false;
    }
}
