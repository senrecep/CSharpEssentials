using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Enums.Net8.Tests;

public class EnumNameLookupTests
{
    private static EnumNameLookup<string> Lookup(StringComparer comparer, params string[] keys) =>
        new(keys.Select((key, i) => new KeyValuePair<string, string>(key, key + "#" + i)), comparer);

    [Fact]
    public void Exact_Lookup_Should_Match_Only_The_Same_Chars()
    {
        EnumNameLookup<string> lookup = Lookup(StringComparer.Ordinal, "pending", "Pending", "in_progress");

        lookup.Find("pending".AsSpan()).Should().Be("pending#0");
        lookup.Find("Pending".AsSpan()).Should().Be("Pending#1");
        lookup.Find("PENDING".AsSpan()).Should().BeNull();
        lookup.Find("in_progress".AsSpan()).Should().Be("in_progress#2");
        lookup.Find("in_progres".AsSpan()).Should().BeNull();
        lookup.Find("in_progress_".AsSpan()).Should().BeNull();
        lookup.Find(ReadOnlySpan<char>.Empty).Should().BeNull();
    }

    [Fact]
    public void Ignore_Case_Lookup_Should_Keep_The_First_Entry()
    {
        EnumNameLookup<string> lookup = Lookup(StringComparer.OrdinalIgnoreCase, "a", "A", "Foo", "FOO", "beta");

        lookup.Find("a".AsSpan()).Should().Be("a#0");
        lookup.Find("A".AsSpan()).Should().Be("a#0");
        lookup.Find("fOo".AsSpan()).Should().Be("Foo#2");
        lookup.Find("BETA".AsSpan()).Should().Be("beta#4");
        lookup.Find("bet".AsSpan()).Should().BeNull();
    }

    [Fact]
    public void Ignore_Case_Lookup_Should_Fold_Non_Ascii_Like_OrdinalIgnoreCase()
    {
        string[] keys = ["straße", "ÉCOLE", "ǅemal", "ΣΙΣΥΦΟΣ", "\U00010400x", "i", "k", "s"];
        string[] probes = ["STRASSE", "STRAßE", "straße", "école", "École", "ǆemal", "Ǆemal", "σισυφος", "σισυφοσ", "\U00010428x", "ı", "İ", "K", "ſ", "x"];
        EnumNameLookup<string> lookup = Lookup(StringComparer.OrdinalIgnoreCase, keys);

        foreach (string probe in probes)
        {
            int index = Array.FindIndex(keys, key => string.Equals(key, probe, StringComparison.OrdinalIgnoreCase));
            lookup.Find(probe.AsSpan()).Should().Be(index < 0 ? null : keys[index] + "#" + index, "probe '{0}'", probe);
        }
    }

    [Fact]
    public void Lookups_Should_Find_Every_Key_Of_A_Large_Table_With_Shared_Buckets()
    {
        string[] keys = [.. Enumerable.Range(0, 500).Select(static i => "member_" + i.ToString(System.Globalization.CultureInfo.InvariantCulture))];
        EnumNameLookup<string> exact = Lookup(StringComparer.Ordinal, keys);
        EnumNameLookup<string> ignoreCase = Lookup(StringComparer.OrdinalIgnoreCase, keys);

        for (int i = 0; i < keys.Length; i++)
        {
            exact.Find(keys[i].AsSpan()).Should().Be(keys[i] + "#" + i);
            ignoreCase.Find(keys[i].ToUpperInvariant().AsSpan()).Should().Be(keys[i] + "#" + i);
            exact.Find(keys[i].ToUpperInvariant().AsSpan()).Should().BeNull();
        }

        exact.Find("member_500".AsSpan()).Should().BeNull();
        ignoreCase.Find(new string('m', 4096).AsSpan()).Should().BeNull();
    }

    [Fact]
    public void Empty_Table_Should_Find_Nothing()
    {
        Lookup(StringComparer.Ordinal).Find("x".AsSpan()).Should().BeNull();
        Lookup(StringComparer.OrdinalIgnoreCase).Find(ReadOnlySpan<char>.Empty).Should().BeNull();
    }

    [Fact]
    public void Other_Comparers_Should_Be_Rejected()
    {
        Action create = () => Lookup(StringComparer.InvariantCultureIgnoreCase, "a");

        create.Should().Throw<ArgumentException>();
    }
}
