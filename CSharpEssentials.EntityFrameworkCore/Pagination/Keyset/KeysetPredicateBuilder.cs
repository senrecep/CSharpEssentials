using System.Linq.Expressions;
using System.Reflection;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>
/// Default, provider-independent <see cref="IKeysetPredicateBuilder"/>. For keys <c>a, b, c</c> it builds the
/// redundant leading-column form <c>a &gt;= @a AND (a &gt; @a OR (a = @a AND (b &gt; @b OR (b = @b AND c &gt; @c))))</c>,
/// with <c>&lt;</c>/<c>&lt;=</c> for descending keys. The extra <c>a &gt;= @a</c> lets the database seek on an index
/// whose leading column is <c>a</c>. A single key gives <c>a &gt; @a</c>.
/// <para>
/// Strings compare with <see cref="string.Compare(string, string)"/>, Guids with <see cref="Guid.CompareTo(Guid)"/>,
/// enums through their underlying numeric type and all other supported types with the comparison operators.
/// </para>
/// </summary>
public sealed class KeysetPredicateBuilder : IKeysetPredicateBuilder
{
    private static readonly MethodInfo StringCompare =
        typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo GuidCompareTo =
        typeof(Guid).GetMethod(nameof(Guid.CompareTo), [typeof(Guid)])!;

    /// <summary>The shared instance.</summary>
    public static KeysetPredicateBuilder Instance { get; } = new();

    /// <summary>Builds the expanded keyset predicate for <paramref name="columns"/>, in key order.</summary>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty.</exception>
    public Expression BuildPredicate(IReadOnlyList<KeysetColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);
        if (columns.Count == 0)
            throw new ArgumentException("At least one keyset column is required.", nameof(columns));

        Expression after = BuildAfter(columns, 0);
        return columns.Count == 1
            ? after
            : Expression.AndAlso(Compare(columns[0], inclusive: true), after);
    }

    private static Expression BuildAfter(IReadOnlyList<KeysetColumn> columns, int index)
    {
        KeysetColumn column = columns[index];
        Expression beyond = Compare(column, inclusive: false);
        if (index == columns.Count - 1)
            return beyond;

        return Expression.OrElse(
            beyond,
            Expression.AndAlso(Expression.Equal(column.Column, column.Value), BuildAfter(columns, index + 1)));
    }

    private static BinaryExpression Compare(KeysetColumn column, bool inclusive)
    {
        (Expression left, Expression right) = Operands(column);
        bool ascending = column.Direction == KeysetDirection.Ascending;
        return (ascending, inclusive) switch
        {
            (true, false) => Expression.GreaterThan(left, right),
            (true, true) => Expression.GreaterThanOrEqual(left, right),
            (false, false) => Expression.LessThan(left, right),
            (false, true) => Expression.LessThanOrEqual(left, right),
        };
    }

    private static (Expression Left, Expression Right) Operands(KeysetColumn column)
    {
        Type type = column.Type;
        if (type == typeof(string))
            return (Expression.Call(StringCompare, column.Column, column.Value), Expression.Constant(0));

        if (type == typeof(Guid))
            return (Expression.Call(column.Column, GuidCompareTo, column.Value), Expression.Constant(0));

        if (type.IsEnum)
        {
            Type underlying = Enum.GetUnderlyingType(type);
            return (Expression.Convert(column.Column, underlying), Expression.Convert(column.Value, underlying));
        }

        return (column.Column, column.Value);
    }
}
