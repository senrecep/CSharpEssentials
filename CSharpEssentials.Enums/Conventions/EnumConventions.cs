namespace CSharpEssentials.Enums;

/// <summary>
/// Every runtime switch of the enum contract. Immutable; register one instance per host.
/// </summary>
/// <remarks>
/// Wire names are decided at build time (<see cref="StringEnumAttribute.Naming"/>), so there is no naming policy here.
/// </remarks>
public sealed record EnumConventions
{
    /// <summary>The default conventions.</summary>
    public static EnumConventions Default { get; } = new();

    /// <summary>Input: accept numbers and numeric strings of defined members.</summary>
    public bool AcceptNumbers { get; init; } = true;

    /// <summary>Input: accept C# member names.</summary>
    public bool AcceptMemberNames { get; init; } = true;

    /// <summary>Input: match wire names, member names and aliases case-insensitively.</summary>
    public bool CaseInsensitive { get; init; } = true;

    /// <summary>Data reads: what to do with an undefined value.</summary>
    public UnknownEnumValueHandling UnknownValue { get; init; } = UnknownEnumValueHandling.UseFallback;

    /// <summary>Output format.</summary>
    public EnumWireFormat WriteAs { get; init; } = EnumWireFormat.String;

    /// <summary>Storage of non-flags enums (EF Core).</summary>
    public EnumStorage Storage { get; init; } = EnumStorage.String;

    /// <summary>Storage of <see cref="FlagsAttribute"/> enums (EF Core).</summary>
    public EnumStorage FlagsStorage { get; init; } = EnumStorage.Integer;

    /// <summary>Whether data layers add check constraints for enum columns.</summary>
    public bool CheckConstraints { get; init; } = true;

    /// <summary>
    /// Which enum types the conventions apply to. Default: enums with generated metadata and <see cref="StringEnumAttribute"/> enums
    /// without it, so a missing generator fails loudly instead of silently writing numbers.
    /// </summary>
    public Func<Type, bool> CanHandle { get; init; } = static type =>
        EnumMetadata.IsRegistered(type) || type is not null && type.IsEnum && type.IsDefined(typeof(StringEnumAttribute), inherit: false);
}
