namespace CSharpEssentials.Enums;

/// <summary>
/// Where a value being read comes from.
/// </summary>
public enum EnumReadMode
{
    /// <summary>
    /// A value sent by a caller (request body, route, query, header, form). Strict: undefined values and the fallback member are rejected,
    /// and the input switches of <see cref="EnumConventions"/> apply.
    /// </summary>
    Input = 0,

    /// <summary>
    /// A value read from something that was valid once (database rows, JSON columns, responses, messages). Tolerant: every known spelling is
    /// accepted and undefined values follow <see cref="EnumConventions.UnknownValue"/>.
    /// </summary>
    Data,
}
