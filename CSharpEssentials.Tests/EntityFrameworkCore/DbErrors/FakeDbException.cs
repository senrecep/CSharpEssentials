using System.Data.Common;

namespace CSharpEssentials.Tests.EntityFrameworkCore.DbErrors;

public sealed class FakeDbException(string? sqlState) : DbException("fake database failure: secret row value")
{
    public override string? SqlState => sqlState;
}
