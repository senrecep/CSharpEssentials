using System.Data.SqlTypes;
using CSharpEssentials.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace CSharpEssentials.Tests.EntityFrameworkCore.OrderedGuids;

/// <summary>Order of each layout under the comparison each database applies to its GUID column.</summary>
public sealed class OrderedGuidLayoutOrderTests
{
    private const int _count = 20_000;

    private static readonly Comparison<Guid> _bigEndianBytes =
        (x, y) => x.ToByteArray(bigEndian: true).AsSpan().SequenceCompareTo(y.ToByteArray(bigEndian: true));

    private static readonly Comparison<Guid> _sqlServer = (x, y) => new SqlGuid(x).CompareTo(new SqlGuid(y));

    private static readonly Comparison<Guid> _sqliteText =
        (x, y) => string.CompareOrdinal(x.ToString().ToUpperInvariant(), y.ToString().ToUpperInvariant());

    private static readonly Comparison<Guid> _mySqlChar36 = (x, y) => string.CompareOrdinal(x.ToString(), y.ToString());

    private static readonly Comparison<Guid> _mySqlLittleEndian =
        (x, y) => x.ToByteArray().AsSpan().SequenceCompareTo(y.ToByteArray());

    private static Guid[] Generate(ValueGeneratorFactory factory)
    {
        ValueGenerator generator = factory.Create(null!, null!);
        return Enumerable.Range(0, _count).Select(_ => (Guid)generator.Next(null!)!).ToArray();
    }

    private static int Inversions(Guid[] ids, Comparison<Guid> comparison)
    {
        int inversions = 0;
        for (int i = 1; i < ids.Length; i++)
            if (comparison(ids[i - 1], ids[i]) >= 0)
                inversions++;
        return inversions;
    }

    [Fact]
    public void Version7_ShouldStrictlyIncrease_InPostgresMySqlBinary16SqliteTextAndChar36Order()
    {
        Guid[] ids = Generate(new Version7GuidValueGeneratorFactory());

        ids.Should().OnlyContain(id => id.Version == 7);
        Inversions(ids, _bigEndianBytes).Should().Be(0);
        Inversions(ids, _sqliteText).Should().Be(0);
        Inversions(ids, _mySqlChar36).Should().Be(0);
        Inversions(ids, (x, y) => x.CompareTo(y)).Should().Be(0);
    }

    [Fact]
    public void Version7_ShouldNotSort_InSqlServerOrLittleEndianBinary16Order()
    {
        Guid[] ids = Generate(new Version7GuidValueGeneratorFactory());

        Inversions(ids, _sqlServer).Should().BeGreaterThan(_count / 4);
        Inversions(ids, _mySqlLittleEndian).Should().BeGreaterThan(0);
    }

    [Fact]
    public void SqlServer_ShouldStrictlyIncrease_InSqlServerOrderOnly()
    {
        Guid[] ids = Generate(new SqlServerGuidValueGeneratorFactory());

        ids.Should().OnlyContain(id => id.Version == 8);
        Inversions(ids, _sqlServer).Should().Be(0);
        Inversions(ids, _bigEndianBytes).Should().BeGreaterThan(_count / 4);
    }

    [Fact]
    public void Generators_ShouldKeepValuesAndBeShared()
    {
        var factory = new SqlServerGuidValueGeneratorFactory();
        ValueGenerator generator = factory.Create(null!, null!);

        generator.GeneratesTemporaryValues.Should().BeFalse();
        factory.Create(null!, null!).Should().BeSameAs(generator);
        new Version7GuidValueGeneratorFactory().Create(null!, null!).GeneratesTemporaryValues.Should().BeFalse();
    }

    [Fact]
    public async Task Generators_ShouldReturnUniqueIncreasingValues_FromManyThreads()
    {
        ValueGenerator generator = new SqlServerGuidValueGeneratorFactory().Create(null!, null!);
        Guid[][] perThread = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() =>
            Enumerable.Range(0, 5_000).Select(_ => (Guid)generator.Next(null!)!).ToArray())));

        perThread.Should().AllSatisfy(ids => Inversions(ids, _sqlServer).Should().Be(0));
        perThread.SelectMany(ids => ids).Distinct().Should().HaveCount(16 * 5_000);
    }

    [Theory]
    [InlineData("Microsoft.EntityFrameworkCore.SqlServer", null, OrderedGuidLayout.SqlServer)]
    [InlineData("Npgsql.EntityFrameworkCore.PostgreSQL", null, OrderedGuidLayout.Version7)]
    [InlineData("Microsoft.EntityFrameworkCore.Sqlite", null, OrderedGuidLayout.Version7)]
    [InlineData("Pomelo.EntityFrameworkCore.MySql", null, OrderedGuidLayout.Version7)]
    [InlineData("MySql.EntityFrameworkCore", null, null)]
    [InlineData("Microsoft.EntityFrameworkCore.InMemory", null, null)]
    [InlineData("Oracle.EntityFrameworkCore", null, null)]
    [InlineData("Microsoft.EntityFrameworkCore.Cosmos", null, null)]
    [InlineData(null, null, null)]
    [InlineData("Microsoft.EntityFrameworkCore.SqlServer", OrderedGuidLayout.Version7, OrderedGuidLayout.Version7)]
    [InlineData("Oracle.EntityFrameworkCore", OrderedGuidLayout.SqlServer, OrderedGuidLayout.SqlServer)]
    public void Resolve_ShouldPickLayoutFromProvider_UnlessGiven(string? provider, OrderedGuidLayout? layout, OrderedGuidLayout? expected) =>
        OrderedGuidKeyConvention.Resolve(layout, provider).Should().Be(expected);
}
