using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Clone;

namespace CSharpEssentials.Baselines.Benchmarks;

/// <summary>
/// Clones every item of a collection into a new materialized collection. The CSE overloads are lazy, so each CSE row calls
/// <c>ToList()</c> or <c>ToArray()</c>. The package has no array overload: the Array row passes the array through the
/// <see cref="IEnumerable{T}"/> overload, typed as such because <c>Clone()</c> on an array variable binds to
/// <see cref="Array.Clone"/>. The Queryable row runs in memory (<c>EnumerableQuery</c>), with no provider translation.
/// Parity checks count, Id sum, name lengths, order and that every result item is a new instance.
/// </summary>
[MemoryDiagnoser]
[SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "ToList and ToArray are the documented idioms being measured; a collection expression compiles to different code.")]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CloneComparison
{
    private const string ListCategory = "List";
    private const string ArrayCategory = "Array";
    private const string LazyCategory = "LazySource";
    private const string QueryableCategory = "Queryable";

    private List<CloneItem> _list = [];
    private CloneItem[] _array = [];
    private IEnumerable<CloneItem> _arrayAsEnumerable = [];
    private IEnumerable<CloneItem> _lazy = [];
    private IQueryable<CloneItem> _queryable = null!;

    [Params(10, 1000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _array = new CloneItem[Count];
        for (int i = 0; i < _array.Length; i++)
            _array[i] = new CloneItem(i, "item-" + i.ToString(CultureInfo.InvariantCulture));
        _list = [.. _array];
        _arrayAsEnumerable = _array;
        _lazy = Iterate(_array);
        _queryable = _list.AsQueryable();

        string expected = Parity.Expected(_array);
        Parity.Same(ListCategory,
            ("expected", expected),
            (nameof(CSharpEssentials_CloneToList), Parity.Of(CSharpEssentials_CloneToList(), _array)),
            (nameof(HandWritten_ForLoop), Parity.Of(HandWritten_ForLoop(), _array)),
            (nameof(Bcl_ConvertAll), Parity.Of(Bcl_ConvertAll(), _array)),
            (nameof(Bcl_LinqSelectToList), Parity.Of(Bcl_LinqSelectToList(), _array)));
        Parity.Same(ArrayCategory,
            ("expected", expected),
            (nameof(CSharpEssentials_CloneToArray), Parity.Of(CSharpEssentials_CloneToArray(), _array)),
            (nameof(HandWritten_ArrayLoop), Parity.Of(HandWritten_ArrayLoop(), _array)),
            (nameof(Bcl_ArrayConvertAll), Parity.Of(Bcl_ArrayConvertAll(), _array)));
        Parity.Same(LazyCategory,
            ("expected", expected),
            (nameof(CSharpEssentials_LazyCloneToList), Parity.Of(CSharpEssentials_LazyCloneToList(), _array)),
            (nameof(HandWritten_LazyForeach), Parity.Of(HandWritten_LazyForeach(), _array)));
        Parity.Same(QueryableCategory,
            ("expected", expected),
            (nameof(CSharpEssentials_QueryableCloneToList), Parity.Of(CSharpEssentials_QueryableCloneToList(), _array)),
            (nameof(HandWritten_ForLoopFromList), Parity.Of(HandWritten_ForLoopFromList(), _array)));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Clone/Readme.MD#cloning-collections
    [Benchmark(Baseline = true), BenchmarkCategory(ListCategory)]
    public List<CloneItem> CSharpEssentials_CloneToList() => _list.Clone().ToList();

    // https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1.-ctor#system-collections-generic-list-1-ctor(system-int32)
    [Benchmark, BenchmarkCategory(ListCategory)]
    public List<CloneItem> HandWritten_ForLoop() => CloneByLoop(_list);

    // https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1.convertall
    [Benchmark, BenchmarkCategory(ListCategory)]
    public List<CloneItem> Bcl_ConvertAll() => _list.ConvertAll(static x => x.Clone());

    // https://learn.microsoft.com/dotnet/api/system.linq.enumerable.select
    [Benchmark, BenchmarkCategory(ListCategory)]
    public List<CloneItem> Bcl_LinqSelectToList() => _list.Select(static x => x.Clone()).ToList();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Clone/Readme.MD#cloning-collections
    [Benchmark(Baseline = true), BenchmarkCategory(ArrayCategory)]
    public CloneItem[] CSharpEssentials_CloneToArray() => _arrayAsEnumerable.Clone().ToArray();

    // https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/arrays
    [Benchmark, BenchmarkCategory(ArrayCategory)]
    public CloneItem[] HandWritten_ArrayLoop()
    {
        CloneItem[] source = _array;
        var result = new CloneItem[source.Length];
        for (int i = 0; i < source.Length; i++)
            result[i] = source[i].Clone();
        return result;
    }

    // https://learn.microsoft.com/dotnet/api/system.array.convertall
    [Benchmark, BenchmarkCategory(ArrayCategory)]
    public CloneItem[] Bcl_ArrayConvertAll() => Array.ConvertAll(_array, static x => x.Clone());

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Clone/Readme.MD#cloning-collections
    [Benchmark(Baseline = true), BenchmarkCategory(LazyCategory)]
    public List<CloneItem> CSharpEssentials_LazyCloneToList() => _lazy.Clone().ToList();

    // https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1.add
    [Benchmark, BenchmarkCategory(LazyCategory)]
    public List<CloneItem> HandWritten_LazyForeach()
    {
        var result = new List<CloneItem>();
        foreach (CloneItem item in _lazy)
            result.Add(item.Clone());
        return result;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Clone/Readme.MD#cloning-collections
    [Benchmark(Baseline = true), BenchmarkCategory(QueryableCategory)]
    public List<CloneItem> CSharpEssentials_QueryableCloneToList() => _queryable.Clone().ToList();

    // https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1.-ctor#system-collections-generic-list-1-ctor(system-int32)
    [Benchmark, BenchmarkCategory(QueryableCategory)]
    public List<CloneItem> HandWritten_ForLoopFromList() => CloneByLoop(_list);

    private static List<CloneItem> CloneByLoop(List<CloneItem> source)
    {
        var result = new List<CloneItem>(source.Count);
        for (int i = 0; i < source.Count; i++)
            result.Add(source[i].Clone());
        return result;
    }

    private static IEnumerable<CloneItem> Iterate(CloneItem[] source)
    {
        foreach (CloneItem item in source)
            yield return item;
    }
}
