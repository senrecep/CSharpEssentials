using System.Linq.Expressions;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>
/// Builds the keyset predicate that selects the rows strictly after the cursor. Replace the default
/// <see cref="KeysetPredicateBuilder"/> through <see cref="KeysetPaginationOptions.PredicateBuilder"/>, for example with a
/// provider-specific row-value comparison.
/// </summary>
public interface IKeysetPredicateBuilder
{
    /// <summary>
    /// Returns a <see cref="bool"/> expression over <paramref name="columns"/>, which are in key order. Keep
    /// <see cref="KeysetColumn.Value"/> in the expression as is so that the cursor values stay SQL parameters.
    /// </summary>
    Expression BuildPredicate(IReadOnlyList<KeysetColumn> columns);
}
