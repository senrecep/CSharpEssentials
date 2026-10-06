using System.Globalization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Builds the SQL expressions of the enum migration helpers from metadata. Every expression is written into the migration as
/// literals, so a later change of the enum never changes an existing migration.
/// </summary>
internal sealed class EnumConversionSql<TEnum> where TEnum : struct, Enum
{
    private const string FlagsSeparator = ", ";

    private readonly EnumInfo<TEnum> _info;
    private readonly EnumSqlDialect _dialect;
    private readonly string _label;
    private readonly List<(string Spelling, EnumMemberInfo<TEnum> Member)> _spellings = [];

    public EnumConversionSql(EnumInfo<TEnum> info, EnumSqlDialect dialect, string label)
    {
        _info = info;
        _dialect = dialect;
        _label = label;

        HashSet<string> seen = [];
        // Earlier spellings win when two members share one: wire names, member names, aliases, legacy names, numbers.
        AddSpellings(seen, member => [member.WireName]);
        AddSpellings(seen, member => [member.MemberName]);
        AddSpellings(seen, member => member.Aliases);
        AddSpellings(seen, member => [EnumColumnCodec<TEnum>.LegacyName(member.MemberName, EnumStoredAs.CamelCase)]);
        AddSpellings(seen, member => [EnumColumnCodec<TEnum>.LegacyName(member.MemberName, EnumStoredAs.LegacySnakeCase)]);
        AddSpellings(seen, member => [member.NumericText]);
    }

    public string IntegerType => _dialect.IntegerType(_info.UnderlyingType);

    /// <summary>Maps any known spelling in the text <paramref name="sql"/> to the name written by <paramref name="format"/>; other values are kept.</summary>
    public string TextToName(string sql, EnumStoredAs? format) =>
        $"CASE {_dialect.Lower(sql)}{string.Concat(_spellings.Select(pair => $" WHEN {EnumSqlDialect.Literal(pair.Spelling)} THEN {EnumSqlDialect.Literal(Name(pair.Member, format))}"))} ELSE {sql} END";

    /// <summary>Maps any known spelling in the text <paramref name="sql"/> to its wire name, anything else to <see langword="null"/>.</summary>
    public string TextToKnownName(string sql) =>
        $"CASE {_dialect.Lower(sql)}{string.Concat(_spellings.Select(pair => $" WHEN {EnumSqlDialect.Literal(pair.Spelling)} THEN {EnumSqlDialect.Literal(Name(pair.Member, null))}"))} END";

    /// <summary>Maps any known spelling in the text <paramref name="sql"/> to its number, anything else to <see langword="null"/>.</summary>
    public string TextToKnownNumber(string sql) => $"CASE {_dialect.Lower(sql)}{NumberWhens()} END";

    /// <summary>
    /// Maps any known spelling in the text <paramref name="sql"/> to its number. Other integer text is kept as that number; any
    /// other text aborts the statement with an error that names the column, the enum and the value.
    /// </summary>
    public string TextToNumber(string sql) =>
        $"CASE {_dialect.Lower(sql)}{NumberWhens()} ELSE {NumberOrFail(sql)} END";

    /// <summary>Like <see cref="TextToNumber"/> for one token of a flags list, where an empty token is zero.</summary>
    public string TokenToNumber(string sql) =>
        $"CASE {_dialect.Lower(sql)}{NumberWhens()} WHEN '' THEN 0 ELSE {NumberOrFail(sql)} END";

    /// <summary>
    /// Maps the number <paramref name="sql"/> to the name written by <paramref name="format"/> (<see langword="null"/> for the wire
    /// name). Flags become their single-flag names joined with <c>", "</c>. Undefined numbers are kept as their text.
    /// </summary>
    public string NumberToName(string sql, EnumStoredAs? format)
    {
        if (!_info.IsFlags)
        {
            string whens = string.Concat(_info.TypedMembers
                .DistinctBy(member => member.RawValue)
                .Select(member => $" WHEN {member.NumericText} THEN {EnumSqlDialect.Literal(Name(member, format))}"));
            return $"CASE {sql}{whens} ELSE {_dialect.ToText(sql)} END";
        }

        EnumMemberInfo<TEnum>[] singles = [.. _info.TypedMembers
            .Where(member => member.RawValue != 0 && (member.RawValue & (member.RawValue - 1)) == 0)
            .DistinctBy(member => member.RawValue)];
        ulong mask = singles.Aggregate(0UL, (current, member) => current | member.RawValue);
        string zero = _info.TypedMembers.FirstOrDefault(member => member.RawValue == 0) is { } none ? Name(none, format) : "0";
        string parts = singles.Length == 0
            ? "''"
            : string.Join(" || ", singles.Select(member =>
                $"CASE WHEN ({sql} & {member.NumericText}) <> 0 THEN {EnumSqlDialect.Literal(FlagsSeparator + Name(member, format))} ELSE '' END"));

        return $"CASE WHEN {sql} IS NULL THEN NULL WHEN {sql} = 0 THEN {EnumSqlDialect.Literal(zero)} " +
            $"WHEN ({sql} & ~{Number(mask)}) <> 0 THEN {_dialect.ToText(sql)} " +
            $"ELSE substr({parts}, {FlagsSeparator.Length + 1}) END";
    }

    /// <summary>The bitwise OR of the comma separated tokens of the text column <paramref name="qualifiedColumn"/>, as a scalar subquery.</summary>
    public string FlagsTextToNumber(string qualifiedColumn)
    {
        if (_dialect.IsPostgres)
        {
            return $"(SELECT coalesce(bit_or({TokenToNumber("cse_token.value")}), 0) " +
                $"FROM unnest(string_to_array({qualifiedColumn}, ',')) AS cse_token(value))";
        }

        // SQLite has no bitwise aggregate: split through json_each, then fold the tokens in order with |.
        return "(WITH RECURSIVE " +
            $"cse_tokens(position, number) AS (SELECT cse_item.key, {TokenToNumber("cse_item.value")} FROM {SqliteTokens(qualifiedColumn)} AS cse_item), " +
            "cse_fold(position, number) AS (SELECT -1, 0 UNION ALL SELECT cse_tokens.position, cse_fold.number | cse_tokens.number " +
            "FROM cse_fold JOIN cse_tokens ON cse_tokens.position = cse_fold.position + 1) " +
            "SELECT number FROM cse_fold ORDER BY position DESC LIMIT 1)";
    }

    /// <summary>True when a comma separated token of the text column <paramref name="qualifiedColumn"/> is no known spelling.</summary>
    public string HasUnknownToken(string qualifiedColumn)
    {
        string known = string.Join(", ", _spellings.Select(pair => EnumSqlDialect.Literal(pair.Spelling)).Append("''"));
        return _dialect.IsPostgres
            ? $"EXISTS (SELECT 1 FROM unnest(string_to_array({qualifiedColumn}, ',')) AS cse_token(value) WHERE {_dialect.Lower("cse_token.value")} NOT IN ({known}))"
            : $"EXISTS (SELECT 1 FROM {SqliteTokens(qualifiedColumn)} AS cse_item WHERE {_dialect.Lower("cse_item.value")} NOT IN ({known}))";
    }

    /// <summary>True when the text <paramref name="sql"/> is not <see langword="null"/> and no known spelling (or token of one, for flags).</summary>
    public string IsUnknownText(string sql)
    {
        string known = string.Join(", ", _spellings.Select(pair => EnumSqlDialect.Literal(pair.Spelling)));
        return $"{sql} IS NOT NULL AND {_dialect.Lower(sql)} NOT IN ({known})";
    }

    /// <summary>True when the number <paramref name="sql"/> is not <see langword="null"/> and no defined value.</summary>
    public string IsUnknownNumber(string sql) =>
        _info.IsFlags
            ? $"{sql} IS NOT NULL AND ({sql} & ~{Number(_info.DefinedMask)}) <> 0"
            : $"{sql} IS NOT NULL AND {sql} NOT IN ({string.Join(", ", _info.TypedMembers.Select(member => member.NumericText).Distinct())})";

    public string Name(EnumMemberInfo<TEnum> member, EnumStoredAs? format)
    {
        EnumMemberInfo<TEnum> canonical = _info.TryGetMember(member.Value, out EnumMemberInfo<TEnum>? found) ? found : member;
        return format is null ? canonical.WireName : EnumColumnCodec<TEnum>.LegacyName(canonical.MemberName, format.Value);
    }

    private static string SqliteTokens(string qualifiedColumn) =>
        $"json_each('[\"' || replace(replace(replace({qualifiedColumn}, '\\', '\\\\'), '\"', '\\\"'), ',', '\",\"') || '\"]')";

    private string NumberWhens() =>
        string.Concat(_spellings.Select(pair => $" WHEN {EnumSqlDialect.Literal(pair.Spelling)} THEN {pair.Member.NumericText}"));

    private string NumberOrFail(string sql) =>
        $"CASE WHEN {_dialect.IsIntegerText(sql)} THEN {_dialect.CastInteger(sql, IntegerType)} " +
        $"ELSE {_dialect.Fail($"CSharpEssentials: cannot convert {_label} to {typeof(TEnum).Name}, unknown value: ", sql, IntegerType)} END";

    private string Number(ulong raw) =>
        Convert.ToString(Convert.ChangeType(_info.FromRawValue(raw), _info.UnderlyingType, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)!;

    private void AddSpellings(HashSet<string> seen, Func<EnumMemberInfo<TEnum>, IEnumerable<string>> spellings)
    {
        foreach (EnumMemberInfo<TEnum> member in _info.TypedMembers)
        {
            foreach (string spelling in spellings(member))
            {
                string lower = string.Concat(spelling.Trim().Select(char.ToLowerInvariant));
                if (seen.Add(lower))
                    _spellings.Add((lower, member));
            }
        }
    }
}
