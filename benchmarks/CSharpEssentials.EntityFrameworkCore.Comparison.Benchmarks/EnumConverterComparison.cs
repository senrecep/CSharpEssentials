using CSharpEssentials.EntityFrameworkCore.Converters;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EfEnumToStringConverter = Microsoft.EntityFrameworkCore.Storage.ValueConversion.EnumToStringConverter<CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks.OrderStatus>;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// Enum column conversion without a database, through the converters' typed delegates, over all five members.
/// The CSE Readme-path rows take their converters from a model built by <see cref="EnumStorageContext"/>:
/// <c>ConfigureEnumConventions</c> installs the wire name converter and <c>HasLegacyEnumStorage(LegacySnakeCase)</c> a
/// converter that keeps the 3.x text. <c>EnumToFormattedStringConverter</c> and <c>LegacySnakeCaseEnumConverter</c> are
/// direct converter classes, not the Readme path, and are labelled as such.
/// Write: EF Core's <c>EnumToStringConverter</c> stores the member name (<c>InTransit</c>); CSE's current converters store
/// the wire name (<c>in_transit</c>) and the legacy ones the 3.x snake case text, so the stored strings differ by design.
/// Read: each converter reads what it wrote; the read tolerance differs (EF also accepts numbers and any casing).
/// ReadLegacy: CSE only, reading 3.x text. The generated metadata of a <c>[StringEnum]</c> lists the 3.x spelling as an
/// alias, so the current converters find it by lookup.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EnumConverterComparison
{
    private const string Write = "Write";
    private const string Read = "Read";
    private const string ReadLegacy = "ReadLegacy";

    private static readonly OrderStatus[] Members = Enum.GetValues<OrderStatus>();

    private Func<OrderStatus, string> _efWrite = null!;
    private Func<OrderStatus, string> _wireWrite = null!;
    private Func<OrderStatus, string> _legacyStorageWrite = null!;
    private Func<OrderStatus, string> _formattedWrite = null!;
    private Func<OrderStatus, string> _legacyWrite = null!;
    private Func<string, OrderStatus> _efRead = null!;
    private Func<string, OrderStatus> _wireRead = null!;
    private Func<string, OrderStatus> _legacyStorageRead = null!;
    private Func<string, OrderStatus> _formattedRead = null!;
    private Func<string, OrderStatus> _legacyRead = null!;
    private string[] _efText = [];
    private string[] _wireText = [];
    private string[] _legacyStorageText = [];
    private string[] _formattedText = [];
    private string[] _legacyText = [];

    internal string Texts { get; private set; } = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        var ef = new EfEnumToStringConverter();
        var formatted = new EnumToFormattedStringConverter<OrderStatus>();
        var legacy = new LegacySnakeCaseEnumConverter<OrderStatus>();
        ValueConverter<OrderStatus, string> wire;
        ValueConverter<OrderStatus, string> legacyStorage;
        using (var context = new EnumStorageContext(new DbContextOptionsBuilder<EnumStorageContext>().UseSqlite("DataSource=:memory:").Options))
        {
            IEntityType shipment = context.Model.FindEntityType(typeof(Shipment))!;
            wire = (ValueConverter<OrderStatus, string>)shipment.FindProperty(nameof(Shipment.Status))!.GetValueConverter()!;
            legacyStorage = (ValueConverter<OrderStatus, string>)shipment.FindProperty(nameof(Shipment.LegacyStatus))!.GetValueConverter()!;
        }

        (_efWrite, _efRead) = (ef.ConvertToProviderTyped, ef.ConvertFromProviderTyped);
        (_wireWrite, _wireRead) = (wire.ConvertToProviderTyped, wire.ConvertFromProviderTyped);
        (_legacyStorageWrite, _legacyStorageRead) = (legacyStorage.ConvertToProviderTyped, legacyStorage.ConvertFromProviderTyped);
        (_formattedWrite, _formattedRead) = (formatted.ConvertToProviderTyped, formatted.ConvertFromProviderTyped);
        (_legacyWrite, _legacyRead) = (legacy.ConvertToProviderTyped, legacy.ConvertFromProviderTyped);
        _efText = [.. Members.Select(_efWrite)];
        _wireText = [.. Members.Select(_wireWrite)];
        _legacyStorageText = [.. Members.Select(_legacyStorageWrite)];
        _formattedText = [.. Members.Select(_formattedWrite)];
        _legacyText = [.. Members.Select(_legacyWrite)];

        string legacyText = _legacyStorageWrite(OrderStatus.Level2Check);
        Parity.True("EnumConverter legacy input",
            legacyText != _wireWrite(OrderStatus.Level2Check) && !string.Equals(legacyText, nameof(OrderStatus.Level2Check), StringComparison.OrdinalIgnoreCase),
            "the legacy spelling of Level2Check matches its wire name or member name, so the legacy input would equal the wire input");
        Parity.Same("EnumConverter written text",
            ("CSharpEssentials ConfigureEnumConventions", string.Join(',', _wireText)),
            ("CSharpEssentials EnumToFormattedStringConverter", string.Join(',', _formattedText)));
        Parity.Same("EnumConverter written legacy text",
            ("CSharpEssentials HasLegacyEnumStorage", string.Join(',', _legacyStorageText)),
            ("CSharpEssentials LegacySnakeCaseEnumConverter", string.Join(',', _legacyText)));
        string expected = string.Join(',', Members);
        Parity.Same("EnumConverter round trip",
            ("Ef", string.Join(',', _efText.Select(_efRead))),
            ("CSharpEssentials wire name", string.Join(',', _wireText.Select(_wireRead))),
            ("CSharpEssentials HasLegacyEnumStorage", string.Join(',', _legacyStorageText.Select(_legacyStorageRead))),
            ("CSharpEssentials formatted", string.Join(',', _formattedText.Select(_formattedRead))),
            ("CSharpEssentials legacy", string.Join(',', _legacyText.Select(_legacyRead))),
            ("CSharpEssentials wire name reading legacy", string.Join(',', _legacyStorageText.Select(_wireRead))),
            ("CSharpEssentials formatted reading legacy", string.Join(',', _legacyStorageText.Select(_formattedRead))),
            ("expected", expected));
        Texts = $"ef [{string.Join(',', _efText)}], wire [{string.Join(',', _wireText)}], legacy storage [{string.Join(',', _legacyStorageText)}]";
    }

    // https://learn.microsoft.com/ef/core/modeling/value-conversions#pre-defined-conversions
    [Benchmark(Baseline = true), BenchmarkCategory(Write)]
    public int Ef_EnumToStringConverter() => Lengths(_efWrite);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#enum-storage
    [Benchmark, BenchmarkCategory(Write)]
    public int CSharpEssentials_EnumWireNameConverter() => Lengths(_wireWrite);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#upgrading-an-existing-database
    [Benchmark, BenchmarkCategory(Write)]
    public int CSharpEssentials_HasLegacyEnumStorage() => Lengths(_legacyStorageWrite);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Converters/EnumToStringConverter.cs
    [Benchmark, BenchmarkCategory(Write)]
    public int CSharpEssentials_EnumToFormattedStringConverter() => Lengths(_formattedWrite);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Converters/LegacySnakeCaseEnumConverter.cs
    [Benchmark, BenchmarkCategory(Write)]
    public int CSharpEssentials_LegacySnakeCaseEnumConverter() => Lengths(_legacyWrite);

    // https://learn.microsoft.com/ef/core/modeling/value-conversions#pre-defined-conversions
    [Benchmark(Baseline = true), BenchmarkCategory(Read)]
    public int Ef_EnumToStringConverter_Read() => Sum(_efRead, _efText);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#enum-storage
    [Benchmark, BenchmarkCategory(Read)]
    public int CSharpEssentials_EnumWireNameConverter_Read() => Sum(_wireRead, _wireText);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#upgrading-an-existing-database
    [Benchmark, BenchmarkCategory(Read)]
    public int CSharpEssentials_HasLegacyEnumStorage_Read() => Sum(_legacyStorageRead, _legacyStorageText);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Converters/EnumToStringConverter.cs
    [Benchmark, BenchmarkCategory(Read)]
    public int CSharpEssentials_EnumToFormattedStringConverter_Read() => Sum(_formattedRead, _formattedText);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Converters/LegacySnakeCaseEnumConverter.cs
    [Benchmark, BenchmarkCategory(Read)]
    public int CSharpEssentials_LegacySnakeCaseEnumConverter_Read() => Sum(_legacyRead, _legacyText);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#enum-storage
    [Benchmark(Baseline = true), BenchmarkCategory(ReadLegacy)]
    public int CSharpEssentials_EnumWireNameConverter_LegacyInput() => Sum(_wireRead, _legacyStorageText);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#upgrading-an-existing-database
    [Benchmark, BenchmarkCategory(ReadLegacy)]
    public int CSharpEssentials_HasLegacyEnumStorage_LegacyInput() => Sum(_legacyStorageRead, _legacyStorageText);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Converters/EnumToStringConverter.cs
    [Benchmark, BenchmarkCategory(ReadLegacy)]
    public int CSharpEssentials_EnumToFormattedStringConverter_LegacyInput() => Sum(_formattedRead, _legacyStorageText);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Converters/LegacySnakeCaseEnumConverter.cs
    [Benchmark, BenchmarkCategory(ReadLegacy)]
    public int CSharpEssentials_LegacySnakeCaseEnumConverter_LegacyInput() => Sum(_legacyRead, _legacyStorageText);

    private static int Lengths(Func<OrderStatus, string> write)
    {
        int total = 0;
        foreach (OrderStatus member in Members)
            total += write(member).Length;
        return total;
    }

    private static int Sum(Func<string, OrderStatus> read, string[] texts)
    {
        int total = 0;
        foreach (string text in texts)
            total += (int)read(text);
        return total;
    }
}
