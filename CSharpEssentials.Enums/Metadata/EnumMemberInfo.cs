namespace CSharpEssentials.Enums;

/// <summary>
/// Typed metadata of one enum member.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class EnumMemberInfo<TEnum> : IEnumMemberInfo where TEnum : struct, Enum
{
    /// <summary>
    /// Creates member metadata. Called by generated code and by <see cref="EnumMetadata.GetOrCreateWithReflection"/>.
    /// </summary>
    /// <param name="value">The member value.</param>
    /// <param name="rawValue">The two's complement bits of the value, sign-extended to 64 bits.</param>
    /// <param name="memberName">The C# identifier.</param>
    /// <param name="wireName">The canonical wire name.</param>
    public EnumMemberInfo(TEnum value, ulong rawValue, string memberName, string wireName)
    {
        Value = value;
        RawValue = rawValue;
        MemberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
        WireName = wireName ?? throw new ArgumentNullException(nameof(wireName));
        NumericText = EnumTypeTraits<TEnum>.FormatRaw(rawValue);
    }

    /// <summary>The member value.</summary>
    public TEnum Value { get; }

    /// <inheritdoc />
    public string MemberName { get; }

    /// <inheritdoc />
    public string WireName { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> Aliases { get; init; } = [];

    /// <inheritdoc />
    public string? Description { get; init; }

    /// <inheritdoc />
    public bool IsObsolete { get; init; }

    /// <inheritdoc />
    public bool IsFallback { get; init; }

    /// <inheritdoc />
    public ulong RawValue { get; }

    /// <inheritdoc />
    public string NumericText { get; }

    /// <inheritdoc />
    public override string ToString() => WireName;
}
