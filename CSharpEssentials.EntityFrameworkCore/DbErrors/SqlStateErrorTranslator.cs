using System.Data.Common;

using CSharpEssentials.Errors;

using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.DbErrors;

/// <summary>
/// Provider-independent <see cref="IDbErrorTranslator"/> that reads <see cref="DbException.SqlState"/> from the exception
/// or any of its inner exceptions. It recognizes integrity violations, serialization failures and deadlocks.
/// <para>
/// The error metadata carries <c>sqlState</c>, <c>entities</c> (the entity types of <see cref="DbUpdateException.Entries"/>,
/// when there are any) and <c>retryable</c> for serialization failures and deadlocks. The database message is not
/// copied into the error, because it can contain row values. Every translated error gets its own metadata instance,
/// so a translator that wraps this one can add entries to it.
/// </para>
/// <para>Only <see cref="Exception.InnerException"/> is followed; the inner exceptions of an <see cref="AggregateException"/> are not searched.</para>
/// </summary>
public sealed class SqlStateErrorTranslator : IDbErrorTranslator
{
    public const string SqlStateKey = "sqlState";
    public const string EntitiesKey = "entities";
    public const string RetryableKey = "retryable";

    public const string UniqueViolation = "23505";
    public const string ForeignKeyViolation = "23503";
    public const string CheckViolation = "23514";
    public const string NotNullViolation = "23502";
    public const string SerializationFailure = "40001";
    public const string DeadlockDetected = "40P01";

    public bool TryTranslate(Exception exception, out Error error)
    {
        ArgumentNullException.ThrowIfNull(exception);

        error = default;
        string? sqlState = FindSqlState(exception);
        if (sqlState is null)
            return false;

        ErrorMetadata metadata = new(SqlStateKey, sqlState);
        if (FindUpdateException(exception) is { } updateException)
            metadata.AddMetadata(EntitiesKey, updateException.Entries.Select(entry => entry.Metadata.DisplayName()).Distinct().ToArray());

        Error? translated = sqlState switch
        {
            UniqueViolation => Error.Conflict("Database.UniqueViolation", "A record with the same unique value already exists.", metadata),
            ForeignKeyViolation => Error.Conflict("Database.ForeignKeyViolation", "The operation conflicts with a related record.", metadata),
            CheckViolation => Error.Validation("Database.CheckViolation", "A value does not satisfy a database constraint.", metadata),
            NotNullViolation => Error.Validation("Database.NotNullViolation", "A required value is missing.", metadata),
            SerializationFailure => Error.Conflict("Database.SerializationFailure", "The operation conflicted with a concurrent transaction and can be retried.", metadata.AddMetadata(RetryableKey, true)),
            DeadlockDetected => Error.Conflict("Database.Deadlock", "The operation was chosen as a deadlock victim and can be retried.", metadata.AddMetadata(RetryableKey, true)),
            _ => null,
        };

        if (translated is null)
            return false;

        error = translated.Value;
        return true;
    }

    private static string? FindSqlState(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbException { SqlState: { Length: > 0 } sqlState })
                return sqlState;
        }

        return null;
    }

    private static DbUpdateException? FindUpdateException(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbUpdateException { Entries.Count: > 0 } updateException)
                return updateException;
        }

        return null;
    }
}
