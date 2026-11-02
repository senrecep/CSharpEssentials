using System.Globalization;
using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums.Runtime;

public class EnumNameLookupTests
{
    private static EnumNameLookup<string> Lookup(StringComparer comparer, params string[] keys) =>
        new(keys.Select((key, i) => new KeyValuePair<string, string>(key, key + "#" + i)), comparer);

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

    [Fact]
    public void Other_Comparers_Should_Be_Rejected()
    {
        Action create = () => Lookup(StringComparer.InvariantCultureIgnoreCase, "a");

        create.Should().Throw<ArgumentException>();
    }
}
