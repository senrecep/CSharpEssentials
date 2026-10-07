namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>Sort direction of a keyset key.</summary>
public enum KeysetDirection
{
    /// <summary>Smallest value first.</summary>
    Ascending = 0,

    /// <summary>Largest value first.</summary>
    Descending = 1,
}
