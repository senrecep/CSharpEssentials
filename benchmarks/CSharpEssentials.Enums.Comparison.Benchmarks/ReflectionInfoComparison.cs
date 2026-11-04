using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>
/// Reads all names (summed lengths) or all values (summed as <see cref="long"/>) of <see cref="PlainStatus"/> when the caller
/// has only a <see cref="Type"/> and the enum carries no attribute. Every CSE row except
/// <c>CSharpEssentials_HeldNames</c> calls <c>EnumMetadata.GetOrCreateWithReflection</c> per invocation, the documented API that builds the info once and caches it.
/// <c>CSharpEssentials_HeldNames</c> is a reference row only: it reads an info held from setup, so the lookup cost is visible.
/// <para>
/// PascalCase wire names equal the member names the BCL and Enums.NET return. The BCL returns a new array per call and
/// <c>Array.GetValue</c> boxes each element, which is part of that non-generic API. CSE lists members in declaration order and
/// Enums.NET in value order; the sums do not depend on order, and <see cref="PlainStatus"/> values ascend anyway.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ReflectionInfoComparison
{
    private const string Names = "Names";
    private const string Values = "Values";

    private readonly Type _type = typeof(PlainStatus);
    private IEnumInfo _held = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (EnumMetadata.IsRegistered(_type))
            throw new InvalidOperationException("PlainStatus has generated metadata, so the CSE rows would not measure the reflection path.");

        _held = EnumMetadata.GetOrCreateWithReflection(_type, EnumNaming.PascalCase);

        int expectedNames = Enum.GetNames<Status>().Sum(static name => name.Length);
        long expectedValues = Enum.GetValues<Status>().Sum(static value => (long)value);
        Parity.Same(Names,
            ("Expected", Parity.Of(expectedNames)),
            (nameof(CSharpEssentials_Names), Parity.Of(CSharpEssentials_Names())),
            (nameof(CSharpEssentials_HeldNames), Parity.Of(CSharpEssentials_HeldNames())),
            (nameof(Bcl_Names), Parity.Of(Bcl_Names())),
            (nameof(EnumsNet_Names), Parity.Of(EnumsNet_Names())));
        Parity.Same(Values,
            ("Expected", Of(expectedValues)),
            (nameof(CSharpEssentials_Values), Of(CSharpEssentials_Values())),
            (nameof(Bcl_Values), Of(Bcl_Values())),
            (nameof(EnumsNet_Values), Of(EnumsNet_Values())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Metadata/EnumMetadata.cs#L64-L81
    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#aot-and-trimming
    [Benchmark(Baseline = true), BenchmarkCategory(Names)]
    public int CSharpEssentials_Names() =>
        Sum(EnumMetadata.GetOrCreateWithReflection(_type, EnumNaming.PascalCase).WireNames);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Metadata/EnumMetadata.cs#L64-L81
    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#aot-and-trimming
    [Benchmark, BenchmarkCategory(Names)]
    public int CSharpEssentials_HeldNames() => Sum(_held.WireNames);

    // https://learn.microsoft.com/dotnet/api/system.enum.getnames#system-enum-getnames(system-type)
    [Benchmark, BenchmarkCategory(Names)]
    public int Bcl_Names() => Sum(Enum.GetNames(_type));

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L3053
    [Benchmark, BenchmarkCategory(Names)]
    public int EnumsNet_Names() => Sum(EnumsNET.Enums.GetNames(_type));

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Metadata/EnumMetadata.cs#L64-L81
    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#aot-and-trimming
    [Benchmark(Baseline = true), BenchmarkCategory(Values)]
    public long CSharpEssentials_Values()
    {
        IReadOnlyList<IEnumMemberInfo> members = EnumMetadata.GetOrCreateWithReflection(_type, EnumNaming.PascalCase).Members;
        long sum = 0;
        for (int i = 0; i < members.Count; i++)
        {
            sum += unchecked((long)members[i].RawValue);
        }

        return sum;
    }

    // https://learn.microsoft.com/dotnet/api/system.enum.getvaluesasunderlyingtype#system-enum-getvaluesasunderlyingtype(system-type)
    [Benchmark, BenchmarkCategory(Values)]
    public long Bcl_Values()
    {
        Array values = Enum.GetValuesAsUnderlyingType(_type);
        long sum = 0;
        for (int i = 0; i < values.Length; i++)
        {
            sum += Convert.ToInt64(values.GetValue(i), CultureInfo.InvariantCulture);
        }

        return sum;
    }

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L3040
    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/EnumMember.cs#L200
    [Benchmark, BenchmarkCategory(Values)]
    public long EnumsNet_Values()
    {
        IReadOnlyList<EnumsNET.EnumMember> members = EnumsNET.Enums.GetMembers(_type);
        long sum = 0;
        for (int i = 0; i < members.Count; i++)
        {
            sum += members[i].ToInt64();
        }

        return sum;
    }

    private static string Of(long value) => value.ToString(CultureInfo.InvariantCulture);

    private static int Sum(string[] names)
    {
        int sum = 0;
        for (int i = 0; i < names.Length; i++)
        {
            sum += names[i].Length;
        }

        return sum;
    }

    private static int Sum(IReadOnlyList<string> names)
    {
        int sum = 0;
        for (int i = 0; i < names.Count; i++)
        {
            sum += names[i].Length;
        }

        return sum;
    }
}
