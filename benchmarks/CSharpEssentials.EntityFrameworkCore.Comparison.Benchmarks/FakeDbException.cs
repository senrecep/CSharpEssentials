using System.Data.Common;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>A provider-neutral <see cref="DbException"/> that reports a SQLSTATE, as Npgsql's exceptions do.</summary>
public sealed class FakeDbException(string? sqlState) : DbException("fake database failure")
{
    public override string? SqlState => sqlState;
}
