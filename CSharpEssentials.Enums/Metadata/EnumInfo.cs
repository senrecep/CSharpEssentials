#if NETSTANDARD2_0
using CharText = string;
#else
using CharText = System.ReadOnlySpan<char>;
#endif
using System.Diagnostics.CodeAnalysis;

namespace CSharpEssentials.Enums;

/// <summary>
/// Typed metadata of one enum type, plus the parser and formatter that every layer uses.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
/// <remarks>
/// Instances are created by generated code (<c>[StringEnum]</c>) and registered in <see cref="EnumMetadata"/>, or created by
/// <see cref="EnumMetadata.GetOrCreateWithReflection"/>.
/// </remarks>
public sealed class EnumInfo<TEnum> : IEnumInfo, IBoxedEnumFormatter where TEnum : struct, Enum
{
    private const int MaxErrorValueLength = 64;

    private readonly EnumMemberInfo<TEnum>[] _members;
    private readonly Func<TEnum, ulong> _toRaw;
    private readonly Func<ulong, TEnum> _fromRaw;
    private readonly Dictionary<ulong, EnumMemberInfo<TEnum>> _byRaw;
    private readonly EnumNameLookup<EnumMemberInfo<TEnum>> _exactAll;
    private readonly EnumNameLookup<EnumMemberInfo<TEnum>> _exactNoMembers;
    private readonly EnumNameLookup<EnumMemberInfo<TEnum>> _ignoreCaseAll;
    private readonly EnumNameLookup<EnumMemberInfo<TEnum>> _ignoreCaseNoMembers;
    private readonly EnumNameLookup<EnumMemberInfo<TEnum>>? _wireExact;
    private readonly ReadOnlyArray<string> _wireNames;
    private readonly ReadOnlyArray<string> _inputWireNames;

    /// <summary>
    /// Creates the metadata. Called by generated code and by <see cref="EnumMetadata.GetOrCreateWithReflection"/>.
    /// </summary>
    /// <param name="members">The members in declaration order.</param>
    /// <param name="toRaw">Converts a value to its sign-extended bits (<c>unchecked((ulong)(long)value)</c>).</param>
    /// <param name="fromRaw">Converts sign-extended bits to a value (<c>unchecked((TEnum)(long)raw)</c>).</param>
    /// <param name="isFlags">Whether the enum is marked <see cref="FlagsAttribute"/>.</param>
    /// <param name="storage">The storage from <see cref="StringEnumAttribute.Storage"/>.</param>
    public EnumInfo(
        IReadOnlyList<EnumMemberInfo<TEnum>> members,
        Func<TEnum, ulong> toRaw,
        Func<ulong, TEnum> fromRaw,
        bool isFlags,
        EnumStorage storage)
    {
        _ = members ?? throw new ArgumentNullException(nameof(members));
        _members = [.. members];
        _toRaw = toRaw ?? throw new ArgumentNullException(nameof(toRaw));
        _fromRaw = fromRaw ?? throw new ArgumentNullException(nameof(fromRaw));
        IsFlags = isFlags;
        Storage = storage;

        _byRaw = [];
        ulong mask = 0;
        foreach (EnumMemberInfo<TEnum> member in _members)
        {
            mask |= member.RawValue;
#if NETSTANDARD2_0
            if (!_byRaw.ContainsKey(member.RawValue))
                _byRaw.Add(member.RawValue, member);
#else
            _ = _byRaw.TryAdd(member.RawValue, member);
#endif
            if (member.IsFallback && Fallback is null)
                Fallback = member;
        }

        DefinedMask = mask;
        string[] wireNames = new string[_members.Length];
        List<string> inputWireNames = [];
        var wires = new KeyValuePair<string, EnumMemberInfo<TEnum>>[_members.Length];
        var names = new KeyValuePair<string, EnumMemberInfo<TEnum>>[_members.Length];
        List<KeyValuePair<string, EnumMemberInfo<TEnum>>> aliases = [];
        for (int i = 0; i < _members.Length; i++)
        {
            EnumMemberInfo<TEnum> member = _members[i];
            wireNames[i] = member.WireName;
            if (!member.IsFallback)
                inputWireNames.Add(member.WireName);
            wires[i] = Pair(member.WireName, member);
            names[i] = Pair(member.MemberName, member);
            foreach (string alias in member.Aliases)
            {
                aliases.Add(Pair(alias, member));
            }
        }

        _wireNames = new(wireNames);
        _inputWireNames = new([.. inputWireNames]);
        _exactAll = new([.. wires, .. names, .. aliases], StringComparer.Ordinal);
        _exactNoMembers = new([.. wires, .. aliases], StringComparer.Ordinal);
        _ignoreCaseAll = new([.. wires, .. names, .. aliases], StringComparer.OrdinalIgnoreCase);
        _ignoreCaseNoMembers = new([.. wires, .. aliases], StringComparer.OrdinalIgnoreCase);
        HashSet<string> ignoreCaseWireNames = [with(StringComparer.OrdinalIgnoreCase), .. wireNames];
        HashSet<string> exactWireNames = [with(StringComparer.Ordinal), .. wireNames];
        if (ignoreCaseWireNames.Count != exactWireNames.Count)
            _wireExact = new(wires, StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public Type EnumType => typeof(TEnum);

    /// <inheritdoc />
    public Type UnderlyingType => Enum.GetUnderlyingType(typeof(TEnum));

    /// <inheritdoc />
    public bool IsFlags { get; }

    /// <inheritdoc />
    public ulong DefinedMask { get; }

    /// <inheritdoc />
    public EnumStorage Storage { get; }

    /// <summary>The typed members in declaration order.</summary>
    public IReadOnlyList<EnumMemberInfo<TEnum>> TypedMembers => _members;

    /// <inheritdoc />
    public IReadOnlyList<IEnumMemberInfo> Members => _members;

    /// <summary>The typed <see cref="EnumFallbackAttribute"/> member, if any.</summary>
    public EnumMemberInfo<TEnum>? Fallback { get; }

    IEnumMemberInfo? IEnumInfo.Fallback => Fallback;

    /// <inheritdoc />
    public IReadOnlyList<string> WireNames => _wireNames;

    /// <inheritdoc />
    public TResult Accept<TResult>(IEnumInfoVisitor<TResult> visitor)
    {
        _ = visitor ?? throw new ArgumentNullException(nameof(visitor));
        return visitor.Visit(this);
    }

    /// <summary>Gets the member declared with <paramref name="value"/> (the first one when several members share it).</summary>
    public bool TryGetMember(TEnum value, [MaybeNullWhen(false)] out EnumMemberInfo<TEnum> member) =>
        _byRaw.TryGetValue(_toRaw(value), out member);

    /// <summary>
    /// Whether <paramref name="value"/> equals a declared member or, for <see cref="FlagsAttribute"/> enums, a combination of declared flags.
    /// </summary>
    public bool IsDefined(TEnum value) => IsDefinedRaw(_toRaw(value));

    /// <summary>The two's complement bits of <paramref name="value"/>, sign-extended to 64 bits.</summary>
    public ulong ToRawValue(TEnum value) => _toRaw(value);

    /// <summary>The value with the sign-extended bits <paramref name="rawValue"/>.</summary>
    public TEnum FromRawValue(ulong rawValue) => _fromRaw(rawValue);

    /// <summary>
    /// Parses one token. See <see cref="EnumValueParser"/> for the lookup order and the read mode rules.
    /// </summary>
    public bool TryParse(CharText text, EnumReadMode mode, EnumConventions conventions, out TEnum value, [NotNullWhen(false)] out EnumValueError? error)
    {
        if (TryParse(text, mode, conventions, out value))
        {
            error = null;
            return true;
        }

        error = CreateError(ToText(text), mode == EnumReadMode.Input);
        return false;
    }

    /// <summary>
    /// Parses one token with the same rules as the overload that reports an <see cref="EnumValueError"/>, without building
    /// the error, so a rejected value allocates nothing.
    /// </summary>
    public bool TryParse(CharText text, EnumReadMode mode, EnumConventions conventions, out TEnum value)
    {
        _ = conventions ?? throw new ArgumentNullException(nameof(conventions));
        return TryParseCore(text, mode, conventions, out value);
    }

    /// <summary>
    /// Parses one token with the <see cref="EnumReadMode.Data"/> rules and <see cref="UnknownEnumValueHandling.Reject"/>: every
    /// known spelling and the number of a defined member are accepted, and an unknown value is never mapped to the fallback member.
    /// Generated <c>TryParseWire</c> calls it, so code that only parses wire values never builds an <see cref="EnumConventions"/>.
    /// </summary>
    public bool TryParseWire(CharText text, out TEnum value) => TryParseCore(text, EnumReadMode.Data, null, out value);

    private bool TryParseCore(CharText text, EnumReadMode mode, EnumConventions? conventions, out TEnum value)
    {
        bool input = mode == EnumReadMode.Input;
        value = default;
#if NETSTANDARD2_0
        if (string.IsNullOrEmpty(text))
            return false;
#else
        if (text.IsEmpty)
            return false;
#endif

        EnumMemberInfo<TEnum>? member = FindByName(text, input, conventions);
        bool ok;
        if (member is not null)
        {
            ok = TryAcceptMember(member, input, out value);
        }
        else if (EnumNumberParser<TEnum>.LooksNumeric(text))
        {
            // A malformed or out of range number is never mapped to the fallback member.
            ok = (!input || conventions is { AcceptNumbers: true }) &&
                 EnumNumberParser<TEnum>.TryParse(text, out ulong raw) &&
                 TryAcceptRaw(raw, mode, conventions, out value);
        }
        else
        {
            // Surrounding whitespace is malformed (no trimming), so it is never mapped to the fallback member either.
            ok = !char.IsWhiteSpace(text[0]) &&
                 !char.IsWhiteSpace(text[text.Length - 1]) &&
                 TryUseFallback(mode, conventions, out value);
        }

        return ok;
    }

    /// <summary>Accepts a JSON or database number. See <see cref="EnumValueParser"/>.</summary>
    public bool TryParseNumber(long number, EnumReadMode mode, EnumConventions conventions, out TEnum value, [NotNullWhen(false)] out EnumValueError? error)
    {
        _ = conventions ?? throw new ArgumentNullException(nameof(conventions));

        value = default;
        bool input = mode == EnumReadMode.Input;
        bool ok = (!input || conventions.AcceptNumbers) &&
                  EnumNumberParser<TEnum>.TryConvert(number, out ulong raw) &&
                  TryAcceptRaw(raw, mode, conventions, out value);
        error = ok ? null : CreateError(number.ToString(System.Globalization.CultureInfo.InvariantCulture), input);
        return ok;
    }

    /// <summary>Accepts a JSON or database number. See <see cref="EnumValueParser"/>.</summary>
    public bool TryParseNumber(ulong number, EnumReadMode mode, EnumConventions conventions, out TEnum value, [NotNullWhen(false)] out EnumValueError? error)
    {
        _ = conventions ?? throw new ArgumentNullException(nameof(conventions));

        value = default;
        bool input = mode == EnumReadMode.Input;
        bool ok = (!input || conventions.AcceptNumbers) &&
                  EnumNumberParser<TEnum>.TryConvert(number, out ulong raw) &&
                  TryAcceptRaw(raw, mode, conventions, out value);
        error = ok ? null : CreateError(number.ToString(System.Globalization.CultureInfo.InvariantCulture), input);
        return ok;
    }

    /// <summary>
    /// Formats a value. <see cref="FlagsAttribute"/> values are written as their single flags joined with <c>,</c> for
    /// <see cref="EnumWireFormat.String"/>; zero is written as the zero member, or as <c>0</c> when there is none. Undefined values throw <see cref="EnumValueException"/>.
    /// </summary>
    public string Format(TEnum value, EnumWireFormat format)
    {
        ulong raw = _toRaw(value);
        if (!IsFlags)
        {
            if (!_byRaw.TryGetValue(raw, out EnumMemberInfo<TEnum>? member))
                throw Undefined(raw);
            return format == EnumWireFormat.Number ? member.NumericText : member.WireName;
        }

        if (!IsDefinedRaw(raw))
            throw Undefined(raw);
        if (format == EnumWireFormat.Number)
            return EnumTypeTraits<TEnum>.FormatRaw(raw);
        // Without a zero member the empty set is written as "0", which reads back as the same value.
        if (raw == 0)
            return _byRaw.TryGetValue(0, out EnumMemberInfo<TEnum>? none) ? none.WireName : "0";

        List<string> names = [];
        Decompose(raw, names);
        return string.Join(",", names);
    }

    /// <summary>
    /// Adds the wire names of the single flags of <paramref name="value"/> to <paramref name="names"/>; zero adds nothing.
    /// Named composites are never written. For a non-flags enum the single wire name is added. Undefined values throw
    /// <see cref="EnumValueException"/>.
    /// </summary>
    public void FormatFlags(TEnum value, IList<string> names)
    {
        _ = names ?? throw new ArgumentNullException(nameof(names));

        if (!IsFlags)
        {
            names.Add(Format(value, EnumWireFormat.String));
            return;
        }

        ulong raw = _toRaw(value);
        if (!IsDefinedRaw(raw))
            throw Undefined(raw);
        Decompose(raw, names);
    }

    string IBoxedEnumFormatter.FormatBoxed(object value, EnumWireFormat format) => Format((TEnum)value, format);

    void IBoxedEnumFormatter.AddFormatted(object value, EnumWireFormat format, IList<string> values)
    {
        if (IsFlags && format == EnumWireFormat.String)
            FormatFlags((TEnum)value, values);
        else
            values.Add(Format((TEnum)value, format));
    }

    /// <summary>
    /// Creates the error for a rejected value; input errors do not list the fallback member. A value longer than 64 chars is
    /// cut to 64 chars followed by <c>…</c>.
    /// </summary>
    public EnumValueError CreateError(string? value, EnumReadMode mode, string? path = null) =>
        new(typeof(TEnum), Truncate(value), mode == EnumReadMode.Input ? _inputWireNames : _wireNames, path);

    private EnumValueError CreateError(string? value, bool input) =>
        CreateError(value, input ? EnumReadMode.Input : EnumReadMode.Data);

    private EnumValueException Undefined(ulong raw) =>
        new(CreateError(EnumTypeTraits<TEnum>.FormatRaw(raw), EnumReadMode.Data));

    private bool IsDefinedRaw(ulong raw) =>
        _byRaw.ContainsKey(raw) || IsFlags && (raw & ~DefinedMask) == 0;

    private EnumMemberInfo<TEnum>? FindByName(CharText text, bool input, EnumConventions? conventions)
    {
        bool ignoreCase = !input || conventions is { CaseInsensitive: true };
        bool members = !input || conventions is { AcceptMemberNames: true };
        if (!ignoreCase)
            return (members ? _exactAll : _exactNoMembers).Find(text);
        return _wireExact?.Find(text) ?? (members ? _ignoreCaseAll : _ignoreCaseNoMembers).Find(text);
    }

    private static bool TryAcceptMember(EnumMemberInfo<TEnum> member, bool input, out TEnum value)
    {
        // The fallback member exists for data reads; a caller can never send it.
        value = member.Value;
        return !(input && member.IsFallback);
    }

    private bool TryAcceptRaw(ulong raw, EnumReadMode mode, EnumConventions? conventions, out TEnum value)
    {
        if (_byRaw.TryGetValue(raw, out EnumMemberInfo<TEnum>? member))
            return TryAcceptMember(member, mode == EnumReadMode.Input, out value);
        if (IsFlags && (raw & ~DefinedMask) == 0)
        {
            value = _fromRaw(raw);
            return true;
        }

        return TryUseFallback(mode, conventions, out value);
    }

    private bool TryUseFallback(EnumReadMode mode, EnumConventions? conventions, out TEnum value)
    {
        if (mode == EnumReadMode.Data && conventions is { UnknownValue: UnknownEnumValueHandling.UseFallback } && Fallback is not null)
        {
            value = Fallback.Value;
            return true;
        }

        value = default;
        return false;
    }

    private void Decompose(ulong raw, IList<string> names)
    {
        ulong remaining = raw;
        foreach (EnumMemberInfo<TEnum> member in _members)
        {
            ulong flag = member.RawValue;
            if (flag != 0 && (flag & (flag - 1)) == 0 && (remaining & flag) != 0)
            {
                names.Add(member.WireName);
                remaining &= ~flag;
            }
        }

        // Bits without a single flag member (CSE0007) are written with the composite that covers them.
        foreach (EnumMemberInfo<TEnum> member in _members)
        {
            ulong flags = member.RawValue;
            if (remaining == 0)
                break;
            if (flags != 0 && (raw & flags) == flags && (remaining & flags) != 0)
            {
                names.Add(member.WireName);
                remaining &= ~flags;
            }
        }

        if (remaining != 0)
            throw Undefined(raw);
    }

    private static KeyValuePair<string, EnumMemberInfo<TEnum>> Pair(string key, EnumMemberInfo<TEnum> member) => new(key, member);

#if NET9_0_OR_GREATER
    private static string? Truncate(string? value) =>
        value is { Length: > MaxErrorValueLength } ? string.Concat(value.AsSpan(0, MaxErrorValueLength), "…") : value;

    private static string ToText(ReadOnlySpan<char> text) =>
        text.Length > MaxErrorValueLength ? string.Concat(text[..MaxErrorValueLength], "…") : text.ToString();
#else
    private static string? Truncate(string? value) =>
        value is { Length: > MaxErrorValueLength } ? value.Substring(0, MaxErrorValueLength) + "…" : value;
#if NETSTANDARD2_0
    private static string ToText(string text) => Truncate(text)!;
#else
    private static string ToText(ReadOnlySpan<char> text) =>
        text.Length > MaxErrorValueLength ? text.Slice(0, MaxErrorValueLength).ToString() + "…" : text.ToString();
#endif
#endif
}
