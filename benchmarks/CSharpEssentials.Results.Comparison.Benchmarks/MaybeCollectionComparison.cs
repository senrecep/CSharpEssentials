using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CseMaybe = CSharpEssentials.Maybe.MaybeExtensions;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class MaybeCollectionComparison
{
    public const string ListSource = "List";
    public const string SequenceSource = "Sequence";

    private const int Size = 1000;
    private const string TryFirstCategory = "TryFirst";
    private const string TryLastCategory = "TryLast";
    private const string TryFirstWhereCategory = "TryFirstWhere";
    private const string TryLastWhereCategory = "TryLastWhere";
    private const string TryFirstMissCategory = "TryFirstMiss";
    private const string TryLastMissCategory = "TryLastMiss";
    private const string TryFindHitCategory = "TryFindHit";
    private const string TryFindMissCategory = "TryFindMiss";
    private const string ChooseCategory = "Choose";

    private IEnumerable<string> _source = [];
    private List<string>? _list;
    private string _target = string.Empty;
    private string _absent = string.Empty;
    private Func<string, bool> _isTarget = null!;
    private Func<string, bool> _isAbsent = null!;
    private Dictionary<string, int> _dictionary = [];
    private Maybe<string>[] _cseMaybes = [];
    private Cfe.Maybe<string>[] _cfeMaybes = [];
    private string?[] _nullables = [];

    [Params(ListSource, SequenceSource)]
    public string Source { get; set; } = ListSource;

    [GlobalSetup]
    public void Setup()
    {
        string[] items = new string[Size];
        for (int i = 0; i < Size; i++)
            items[i] = "item" + i.ToString("D4", CultureInfo.InvariantCulture);
        _list = Source == ListSource ? [.. items] : null;
        _source = _list ?? (IEnumerable<string>)new StringSequence(items);
        _target = ErrorInputs.Copy("item0500");
        _absent = "absent";
        _isTarget = s => s == _target;
        _isAbsent = s => s == _absent;
        _dictionary = new Dictionary<string, int>(Size, StringComparer.Ordinal);
        _cseMaybes = new Maybe<string>[Size];
        _cfeMaybes = new Cfe.Maybe<string>[Size];
        _nullables = new string?[Size];
        for (int i = 0; i < Size; i++)
        {
            _dictionary.Add(items[i], i);
            bool some = i % 2 == 0;
            _cseMaybes[i] = some ? Maybe<string>.From(items[i]) : Maybe<string>.None;
            _cfeMaybes[i] = some ? Cfe.Maybe<string>.From(items[i]) : Cfe.Maybe<string>.None;
            _nullables[i] = some ? items[i] : null;
        }

        Check(TryFirstCategory, "ok:item0000",
            ErrorParity.Of(CSharpEssentials_TryFirst()), ErrorParity.Of(CSharpFunctionalExtensions_TryFirst()));
        Check(TryLastCategory, "ok:item0999",
            ErrorParity.Of(CSharpEssentials_TryLast()), ErrorParity.Of(CSharpFunctionalExtensions_TryLast()));
        Check(TryFirstWhereCategory, "ok:item0500",
            ErrorParity.Of(CSharpEssentials_TryFirstWhere()), ErrorParity.Of(CSharpFunctionalExtensions_TryFirstWhere()),
            ErrorParity.Found(HandWritten_TryFirstWhere()));
        Check(TryLastWhereCategory, "ok:item0500",
            ErrorParity.Of(CSharpEssentials_TryLastWhere()), ErrorParity.Of(CSharpFunctionalExtensions_TryLastWhere()),
            ErrorParity.Found(HandWritten_TryLastWhere()));
        Check(TryFirstMissCategory, "none",
            ErrorParity.Of(CSharpEssentials_TryFirstMiss()), ErrorParity.Of(CSharpFunctionalExtensions_TryFirstMiss()),
            ErrorParity.Found(HandWritten_TryFirstMiss()));
        Check(TryLastMissCategory, "none",
            ErrorParity.Of(CSharpEssentials_TryLastMiss()), ErrorParity.Of(CSharpFunctionalExtensions_TryLastMiss()),
            ErrorParity.Found(HandWritten_TryLastMiss()));
        Check(TryFindHitCategory, "ok:500",
            Parity.Of(CSharpEssentials_TryFindHit()), Parity.Of(CSharpFunctionalExtensions_TryFindHit()),
            ErrorParity.Found(HandWritten_TryFindHit()));
        Check(TryFindMissCategory, "none",
            Parity.Of(CSharpEssentials_TryFindMiss()), Parity.Of(CSharpFunctionalExtensions_TryFindMiss()),
            ErrorParity.Found(HandWritten_TryFindMiss()));
        Check(ChooseCategory, "ok:4000",
            Parity.Of(CSharpEssentials_Choose()), Parity.Of(CSharpFunctionalExtensions_Choose()),
            Parity.Of(HandWritten_Choose()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(TryFirstCategory)]
    public Maybe<string> CSharpEssentials_TryFirst() => CseMaybe.TryFirst(_source);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#tryfirst-and-trylast
    [Benchmark, BenchmarkCategory(TryFirstCategory)]
    public Cfe.Maybe<string> CSharpFunctionalExtensions_TryFirst() => Cfe.MaybeExtensions.TryFirst(_source);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(TryLastCategory)]
    public Maybe<string> CSharpEssentials_TryLast() => CseMaybe.TryLast(_source);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#tryfirst-and-trylast
    [Benchmark, BenchmarkCategory(TryLastCategory)]
    public Cfe.Maybe<string> CSharpFunctionalExtensions_TryLast() => Cfe.MaybeExtensions.TryLast(_source);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(TryFirstWhereCategory)]
    public Maybe<string> CSharpEssentials_TryFirstWhere() => CseMaybe.TryFirst(_source, _isTarget);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#tryfirst-and-trylast
    [Benchmark, BenchmarkCategory(TryFirstWhereCategory)]
    public Cfe.Maybe<string> CSharpFunctionalExtensions_TryFirstWhere() => Cfe.MaybeExtensions.TryFirst(_source, _isTarget);

    [Benchmark, BenchmarkCategory(TryFirstWhereCategory)]
    public string? HandWritten_TryFirstWhere() => FindFirst(_isTarget);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(TryLastWhereCategory)]
    public Maybe<string> CSharpEssentials_TryLastWhere() => CseMaybe.TryLast(_source, _isTarget);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#tryfirst-and-trylast
    [Benchmark, BenchmarkCategory(TryLastWhereCategory)]
    public Cfe.Maybe<string> CSharpFunctionalExtensions_TryLastWhere() => Cfe.MaybeExtensions.TryLast(_source, _isTarget);

    [Benchmark, BenchmarkCategory(TryLastWhereCategory)]
    public string? HandWritten_TryLastWhere() => FindLast(_isTarget);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(TryFirstMissCategory)]
    public Maybe<string> CSharpEssentials_TryFirstMiss() => CseMaybe.TryFirst(_source, _isAbsent);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#tryfirst-and-trylast
    [Benchmark, BenchmarkCategory(TryFirstMissCategory)]
    public Cfe.Maybe<string> CSharpFunctionalExtensions_TryFirstMiss() => Cfe.MaybeExtensions.TryFirst(_source, _isAbsent);

    [Benchmark, BenchmarkCategory(TryFirstMissCategory)]
    public string? HandWritten_TryFirstMiss() => FindFirst(_isAbsent);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(TryLastMissCategory)]
    public Maybe<string> CSharpEssentials_TryLastMiss() => CseMaybe.TryLast(_source, _isAbsent);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#tryfirst-and-trylast
    [Benchmark, BenchmarkCategory(TryLastMissCategory)]
    public Cfe.Maybe<string> CSharpFunctionalExtensions_TryLastMiss() => Cfe.MaybeExtensions.TryLast(_source, _isAbsent);

    [Benchmark, BenchmarkCategory(TryLastMissCategory)]
    public string? HandWritten_TryLastMiss() => FindLast(_isAbsent);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(TryFindHitCategory)]
    public Maybe<int> CSharpEssentials_TryFindHit() => CseMaybe.TryFind(_dictionary, _target);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#tryfind
    [Benchmark, BenchmarkCategory(TryFindHitCategory)]
    public Cfe.Maybe<int> CSharpFunctionalExtensions_TryFindHit() => Cfe.MaybeExtensions.TryFind(_dictionary, _target);

    [Benchmark, BenchmarkCategory(TryFindHitCategory)]
    public int? HandWritten_TryFindHit() => _dictionary.TryGetValue(_target, out int value) ? value : null;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(TryFindMissCategory)]
    public Maybe<int> CSharpEssentials_TryFindMiss() => CseMaybe.TryFind(_dictionary, _absent);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#tryfind
    [Benchmark, BenchmarkCategory(TryFindMissCategory)]
    public Cfe.Maybe<int> CSharpFunctionalExtensions_TryFindMiss() => Cfe.MaybeExtensions.TryFind(_dictionary, _absent);

    [Benchmark, BenchmarkCategory(TryFindMissCategory)]
    public int? HandWritten_TryFindMiss() => _dictionary.TryGetValue(_absent, out int value) ? value : null;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(ChooseCategory)]
    public int CSharpEssentials_Choose()
    {
        int sum = 0;
        foreach (string value in CseMaybe.Choose(_cseMaybes))
            sum += value.Length;
        return sum;
    }

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#choose
    [Benchmark, BenchmarkCategory(ChooseCategory)]
    public int CSharpFunctionalExtensions_Choose()
    {
        int sum = 0;
        foreach (string value in Cfe.MaybeExtensions.Choose(_cfeMaybes))
            sum += value.Length;
        return sum;
    }

    [Benchmark, BenchmarkCategory(ChooseCategory)]
    public int HandWritten_Choose()
    {
        int sum = 0;
        foreach (string? value in _nullables)
        {
            if (value is not null)
                sum += value.Length;
        }

        return sum;
    }

    private static void Check(string category, string expected, params string[] outcomes)
    {
        string[] libraries = [nameof(CSharpEssentials), nameof(CSharpFunctionalExtensions), "HandWritten"];
        var pairs = new (string Library, string Outcome)[outcomes.Length + 1];
        pairs[0] = ("Expected", expected);
        for (int i = 0; i < outcomes.Length; i++)
            pairs[i + 1] = (libraries[i], outcomes[i]);
        Parity.Same(nameof(MaybeCollectionComparison) + "." + category, pairs);
    }

    private string? FindFirst(Func<string, bool> predicate)
    {
        if (_list is { } list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    return list[i];
            }

            return null;
        }

        foreach (string item in _source)
        {
            if (predicate(item))
                return item;
        }

        return null;
    }

    private string? FindLast(Func<string, bool> predicate)
    {
        if (_list is { } list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                    return list[i];
            }

            return null;
        }

        string? last = null;
        foreach (string item in _source)
        {
            if (predicate(item))
                last = item;
        }

        return last;
    }
}
