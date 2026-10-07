#if NET10_0_OR_GREATER
namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Conventional keys for EF Core 10 named query filters. Pass them to <c>HasQueryFilter(key, filter)</c> when configuring a filter,
/// and to <c>IgnoreQueryFilters(new[] { key })</c> (a cached array; see <see cref="NamedQueryFilterExtensions.IgnoreSoftDeleteQueryFilter{TEntity}"/>) to switch off only that filter for one query while every other filter (for example the
/// tenant filter) stays active.
/// </summary>
public static class QueryFilterNames
{
    /// <summary>
    /// Key of the soft-delete filter that hides rows whose <c>IsDeleted</c> is <see langword="true"/>.
    /// <see cref="NamedQueryFilterExtensions.HasSoftDeleteQueryFilter{TEntity}"/> and
    /// <see cref="NamedQueryFilterExtensions.ApplyNamedSoftDeleteQueryFilter"/> register the filter under this key.
    /// </summary>
    public const string SoftDelete = "SoftDelete";

    /// <summary>
    /// Key of the multi-tenancy filter that limits rows to the current tenant. The tenant predicate depends on the application,
    /// so this package does not register it; use this key when you do.
    /// </summary>
    public const string Tenant = "Tenant";
}
#endif
