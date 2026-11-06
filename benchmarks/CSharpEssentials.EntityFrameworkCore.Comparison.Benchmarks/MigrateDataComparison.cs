using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// Syncs a countries table of <see cref="Count"/> rows to seed data that drops 10% of them, renames 10% and adds 10% new
/// codes. Every invocation creates a context, opens a transaction, syncs, saves and rolls back. The hand-written row loads
/// a dictionary and diffs against a set of seed codes; CSE's <c>MigrateDataAsync</c> runs with the Readme's options.
/// Both send the same DELETE, full-row UPDATE and INSERT statements. Parity: after the save, the row count and a checksum
/// of every (Code, Name) pair match.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the database and contexts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class MigrateDataComparison
{
    private static readonly MigrateDataOptions<Country, CountrySeed, string> Options = new()
    {
        Query = set => set,
        EntityKeyProperty = c => c.Code,
        DataKeyProperty = s => s.Code,
        IsUpdatedFunc = (c, s) => c.Name != s.Name,
        UpdateFunc = (c, s) =>
        {
            c.Name = s.Name;
            return c;
        },
        Converter = s => new Country { Code = s.Code, Name = s.Name },
        HardDeleteMode = false,
    };

    private BenchDatabase _database = null!;
    private CountrySeed[] _seed = [];

    [Params(1_000, 10_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _database = BenchDatabase.Create();
        using (PlainContext context = _database.Plain())
        {
            context.Set<Country>().AddRange(Enumerable.Range(0, Count).Select(static i => new Country { Code = Code(i), Name = Name(i) }));
            context.SaveChanges();
        }

        _seed =
        [
            .. Enumerable.Range(0, Count)
                .Where(static i => i % 10 != 0)
                .Select(static i => new CountrySeed(Code(i), i % 10 == 1 ? Name(i) + " renamed" : Name(i))),
            .. Enumerable.Range(Count, Count / 10).Select(static i => new CountrySeed(Code(i), Name(i))),
        ];
        Verify().GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void Cleanup() => _database.Dispose();

    // https://learn.microsoft.com/ef/core/saving/basic
    [Benchmark(Baseline = true)]
    public Task<string> HandWritten_DictionaryDiff() => Run(HandWrittenSync, verify: false);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#result-helpers-hard-delete-and-seeding
    [Benchmark]
    public Task<string> CSharpEssentials_MigrateDataAsync() => Run(CseSync, verify: false);

    private async Task HandWrittenSync(PlainContext context)
    {
        DbSet<Country> countries = context.Set<Country>();
        Dictionary<string, Country> existing = await countries.ToDictionaryAsync(static c => c.Code, StringComparer.Ordinal);
        HashSet<string> seeded = new(_seed.Select(static s => s.Code), StringComparer.Ordinal);
        List<Country> updated = [];
        List<Country> added = [];
        foreach (CountrySeed seed in _seed)
        {
            if (!existing.TryGetValue(seed.Code, out Country? country))
            {
                added.Add(new Country { Code = seed.Code, Name = seed.Name });
            }
            else if (country.Name != seed.Name)
            {
                country.Name = seed.Name;
                updated.Add(country);
            }
        }

        countries.RemoveRange(existing.Values.Where(c => !seeded.Contains(c.Code)));
        countries.UpdateRange(updated);
        countries.AddRange(added);
        await context.SaveChangesAsync();
    }

    private Task CseSync(PlainContext context) => context.MigrateDataAsync(_seed, Options);

    private async Task<string> Run(Func<PlainContext, Task> sync, bool verify)
    {
        await using PlainContext context = _database.Plain();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        await sync(context);
        string checksum = verify ? await Checksum(context) : string.Empty;
        await transaction.RollbackAsync();
        return checksum;
    }

    private static async Task<string> Checksum(PlainContext context)
    {
        List<string> rows = await context.Set<Country>().AsNoTracking().OrderBy(c => c.Code).Select(c => c.Code + "=" + c.Name).ToListAsync();
        return $"{rows.Count}|{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(';', rows))))}";
    }

    private async Task Verify()
    {
        string scenario = $"MigrateData Count={Count}";
        string handWritten = await Run(HandWrittenSync, verify: true);
        Parity.True(scenario, handWritten.StartsWith($"{_seed.Length}|", StringComparison.Ordinal), $"the synced table has {handWritten} rows, not {_seed.Length}");
        Parity.Same(scenario, ("HandWritten", handWritten), ("CSharpEssentials", await Run(CseSync, verify: true)));
    }

    private static string Code(int i) => $"C{i:D6}";

    private static string Name(int i) => $"Country {i}";
}
