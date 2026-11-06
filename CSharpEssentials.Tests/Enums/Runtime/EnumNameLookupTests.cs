using System.Globalization;
using System.Reflection;
using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums.Runtime;

public class EnumNameLookupTests
{
    private static EnumNameLookup<string> Lookup(StringComparer comparer, params string[] keys) =>
        new(keys.Select((key, i) => new KeyValuePair<string, string>(key, key + "#" + i)), comparer);

    private static int LongestChain(EnumNameLookup<string> lookup)
    {
        int[] buckets = Field<int[]>(lookup, "_buckets");
        int[] next = Field<int[]>(lookup, "_next");
        int longest = 0;
        foreach (int head in buckets)
        {
            int length = 0;
            for (int i = head - 1; i >= 0; i = next[i])
                length++;
            longest = Math.Max(longest, length);
        }

        return longest;
    }

    private static T Field<T>(object target, string name) =>
        (T)target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(target)!;

    [Fact]
    public void OrdinalIgnoreCase_Should_Never_Equate_A_Non_Ascii_Char_With_An_Ascii_Char()
    {
        List<string> matches = [];
        for (int c = 0x80; c <= char.MaxValue; c++)
        {
            char nonAscii = (char)c;
            for (int a = 0; a < 0x80; a++)
            {
                char ascii = (char)a;
                if (MemoryExtensions.Equals([nonAscii], [ascii], StringComparison.OrdinalIgnoreCase))
                    matches.Add($"U+{c:X4} = U+{a:X4}");
            }
        }

        matches.Should().BeEmpty();
    }

    [Fact]
    public void Lookups_Should_Agree_With_Dictionary_For_Mixed_Ascii_And_Non_Ascii_Keys()
    {
        string[] keys =
        [
            "pending", "Pending", "in_progress", "straße", "ÉCOLE", "ǅemal", "ΣΙΣΥΦΟΣ", "\U00010400x", "i", "k", "s", "a@b", "a`b",
            "KELVIN", "Kelvin", "ıd", "id", "ſtate", "state", "x", "",
        ];
        string[] probes =
        [
            .. keys, "PENDING", "IN_PROGRESS", "STRASSE", "STRAßE", "école", "ǆemal", "Ǆemal", "σισυφος", "\U00010428x", "ı", "İ", "K",
            "ſ", "A@B", "A`B", "a@B", "kelvin", "ID", "İD", "STATE", "ſTATE", "y", "pendinG ", "K",
        ];
        foreach (StringComparer comparer in new[] { StringComparer.Ordinal, StringComparer.OrdinalIgnoreCase })
        {
            Dictionary<string, string> expected = new(comparer);
            for (int i = 0; i < keys.Length; i++)
                expected.TryAdd(keys[i], keys[i] + "#" + i);
            EnumNameLookup<string> lookup = Lookup(comparer, keys);

            foreach (string probe in probes)
                lookup.Find(probe.AsSpan()).Should().Be(expected.GetValueOrDefault(probe), "probe '{0}'", probe);
        }
    }

    [Theory]
    [InlineData("in_transit")]
    [InlineData("In_Transit")]
    [InlineData("IN_TRANSIT")]
    [InlineData("level2_check")]
    [InlineData("LEVEL2_CHECK")]
    public void IgnoreCase_Lookup_Should_Return_The_First_Added_Entry_For_Exact_And_Case_Variant_Input(string probe)
    {
        EnumNameLookup<string> lookup = Lookup(StringComparer.OrdinalIgnoreCase, "In_Transit", "in_transit", "level2_check", "Level2_Check");

        lookup.Find(probe.AsSpan()).Should().Be(probe.StartsWith("in", StringComparison.OrdinalIgnoreCase) ? "In_Transit#0" : "level2_check#2");
        Lookup(StringComparer.Ordinal, "In_Transit", "level2_check").Find(probe.AsSpan())
            .Should().Be(probe is "In_Transit" ? "In_Transit#0" : probe is "level2_check" ? "level2_check#1" : null);
    }

    [Fact]
    public void Lookups_Should_Find_Every_Key_Of_A_Large_Table_Of_Similar_Names()
    {
        string[] keys = [.. Enumerable.Range(0, 2000).Select(static i => "Code" + i.ToString("D4", CultureInfo.InvariantCulture))];
        EnumNameLookup<string> exact = Lookup(StringComparer.Ordinal, keys);
        EnumNameLookup<string> ignoreCase = Lookup(StringComparer.OrdinalIgnoreCase, keys);

        for (int i = 0; i < keys.Length; i++)
        {
            exact.Find(keys[i].AsSpan()).Should().Be(keys[i] + "#" + i);
            ignoreCase.Find(keys[i].ToUpperInvariant().AsSpan()).Should().Be(keys[i] + "#" + i);
            exact.Find(keys[i].ToUpperInvariant().AsSpan()).Should().BeNull();
        }

        exact.Find("Code2000".AsSpan()).Should().BeNull();
        ignoreCase.Find(new string('c', 4096).AsSpan()).Should().BeNull();
    }

    [Theory]
    [InlineData("Error_{0:D3}_Code")]
    [InlineData("Level{0:D2}x")]
    public void Names_That_Differ_Only_Between_Sampled_Chars_Should_Not_Chain(string format)
    {
        string[] keys = [.. Enumerable.Range(0, 100).Select(i => string.Format(CultureInfo.InvariantCulture, format, i))];
        foreach (StringComparer comparer in new[] { StringComparer.Ordinal, StringComparer.OrdinalIgnoreCase })
        {
            EnumNameLookup<string> lookup = Lookup(comparer, keys);

            for (int i = 0; i < keys.Length; i++)
                lookup.Find(keys[i].AsSpan()).Should().Be(keys[i] + "#" + i);
            LongestChain(lookup).Should().BeLessThanOrEqualTo(4);
        }
    }

    [Fact]
    public void Other_Comparers_Should_Be_Rejected()
    {
        Action create = () => Lookup(StringComparer.InvariantCultureIgnoreCase, "a");

        create.Should().Throw<ArgumentException>();
    }
}
