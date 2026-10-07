using System.Linq.Expressions;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>One key of a keyset predicate, as passed to <see cref="IKeysetPredicateBuilder"/>.</summary>
public sealed class KeysetColumn
{
    internal KeysetColumn(Expression column, Expression value, KeysetDirection direction)
    {
        Column = column;
        Value = value;
        Direction = direction;
    }

    /// <summary>The key expression, bound to the predicate's single lambda parameter.</summary>
    public Expression Column { get; }

    /// <summary>
    /// The cursor value of this key. It reads a member of a captured object, so EF Core sends it as a SQL parameter.
    /// Use it in the predicate as is; do not evaluate it into a constant.
    /// </summary>
    public Expression Value { get; }

    /// <summary>The key type, which is the type of both <see cref="Column"/> and <see cref="Value"/>.</summary>
    public Type Type => Column.Type;

    /// <summary>
    /// The direction in which this page is read. Rows after the cursor have a greater value for
    /// <see cref="KeysetDirection.Ascending"/> and a smaller one for <see cref="KeysetDirection.Descending"/>.
    /// For a <c>Before</c> page the configured direction is already reversed.
    /// </summary>
    public KeysetDirection Direction { get; }
}
