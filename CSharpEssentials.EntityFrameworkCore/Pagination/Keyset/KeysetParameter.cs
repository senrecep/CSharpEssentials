namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>Captured holder whose member access EF Core turns into a SQL parameter instead of an inlined constant.</summary>
internal sealed class KeysetParameter<TValue>(TValue value)
{
    public TValue Value { get; } = value;
}
