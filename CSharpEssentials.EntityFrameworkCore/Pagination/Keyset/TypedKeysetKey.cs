using System.Linq.Expressions;
using System.Reflection;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

internal sealed class TypedKeysetKey<T, TKey> : KeysetKey<T>
{
    private readonly MemberInfo[] _members;
    private readonly Expression<Func<T, TKey>> _selector;

    private TypedKeysetKey(Expression<Func<T, TKey>> selector, MemberInfo[] members, string path, KeysetDirection direction)
        : base(selector, path, direction)
    {
        _selector = selector;
        _members = members;
    }

    public static TypedKeysetKey<T, TKey> Create(Expression<Func<T, TKey>> selector, KeysetDirection direction)
    {
        ArgumentNullException.ThrowIfNull(selector);

        MemberInfo[] members = ReadMemberChain(selector);
        string path = string.Join('.', members.Select(member => member.Name));
        Type type = typeof(TKey);

        if (Nullable.GetUnderlyingType(type) is not null)
            throw new ArgumentException(
                $"Keyset key '{path}' is nullable ('{Nullable.GetUnderlyingType(type)!.Name}?'). Nullable key columns are not supported; " +
                "use a non-nullable column or project a non-null value before paging.", nameof(selector));

        if (!KeysetValueSerializer.IsSupported(type))
            throw new ArgumentException(
                $"Keyset key '{path}' has the unsupported type '{type.Name}'. Supported key types are int, long, short, byte, decimal, " +
                "double, float, string, Guid, DateTime, DateTimeOffset, TimeSpan, DateOnly, TimeOnly and enums whose underlying type is not ulong.",
                nameof(selector));

        if (!type.IsValueType && IsAnnotatedNullable(members[^1]))
            throw new ArgumentException(
                $"Keyset key '{path}' is a nullable reference ('{type.Name}?'). Nullable key columns are not supported.", nameof(selector));

        MemberInfo? nullableNavigation = members[..^1].FirstOrDefault(IsNullable);
        if (nullableNavigation is not null)
            throw new ArgumentException(
                $"Keyset key '{path}' goes through the nullable reference '{nullableNavigation.Name}'. Every member of a key path must be non-nullable.",
                nameof(selector));

        return new TypedKeysetKey<T, TKey>(selector, members, path, direction);
    }

    public override object GetValue(T item)
    {
        object? current = item;
        foreach (MemberInfo member in _members)
        {
            if (current is null)
                break;
            current = member is PropertyInfo property ? property.GetValue(current) : ((FieldInfo)member).GetValue(current);
        }

        return current ?? throw new InvalidOperationException(
            $"Keyset key '{Path}' evaluated to null for a row of '{typeof(T).Name}'. Keyset keys must never be null.");
    }

    public override Expression CreateValue(object value) =>
        Expression.Property(
            Expression.Constant(new KeysetParameter<TKey>((TKey)value)),
            nameof(KeysetParameter<>.Value));

    public override IOrderedQueryable<T> ApplyOrder(IQueryable<T> source, bool first, bool descending)
    {
        if (first)
            return descending ? source.OrderByDescending(_selector) : source.OrderBy(_selector);

        var ordered = (IOrderedQueryable<T>)source;
        return descending ? ordered.ThenByDescending(_selector) : ordered.ThenBy(_selector);
    }

    private static MemberInfo[] ReadMemberChain(Expression<Func<T, TKey>> selector)
    {
        List<MemberInfo> members = [];
        Expression? current = selector.Body;
        while (current is MemberExpression { Member: PropertyInfo or FieldInfo } member)
        {
            members.Add(member.Member);
            current = member.Expression;
        }

        if (current != selector.Parameters[0] || members.Count == 0)
            throw new ArgumentException(
                $"Keyset key '{selector}' must be a property or field access on the lambda parameter, such as 'x => x.CreatedAt'.",
                nameof(selector));

        members.Reverse();
        return [.. members];
    }

    private static bool IsNullable(MemberInfo member)
    {
        Type type = member is PropertyInfo property ? property.PropertyType : ((FieldInfo)member).FieldType;
        return type.IsValueType ? Nullable.GetUnderlyingType(type) is not null : IsAnnotatedNullable(member);
    }

    private static bool IsAnnotatedNullable(MemberInfo member)
    {
        NullabilityInfoContext context = new();
        NullabilityInfo info = member is PropertyInfo property ? context.Create(property) : context.Create((FieldInfo)member);
        return info.ReadState == NullabilityState.Nullable;
    }
}
