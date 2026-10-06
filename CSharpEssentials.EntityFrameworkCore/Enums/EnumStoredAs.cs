namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// The format an existing enum column holds (design section 12). <c>HasLegacyEnumStorage</c> keeps writing it; reads accept
/// every spelling.
/// </summary>
public enum EnumStoredAs
{
    /// <summary>The underlying number: <c>0</c>, <c>1</c>.</summary>
    Integer = 0,

    /// <summary>The C# member name: <c>PendingApproval</c>.</summary>
    MemberName,

    /// <summary>The camelCase member name: <c>pendingApproval</c>.</summary>
    CamelCase,

    /// <summary>The 3.x format, CSharpEssentials.Core <c>ToSnakeCase</c>: <c>HTTPStatus</c> → <c>httpstatus</c>.</summary>
    LegacySnakeCase,

    /// <summary>Any known spelling. Valid only as the source of a conversion, never as a write format.</summary>
    Text,

    /// <summary>Flags as member names joined with <c>", "</c>: <c>Read, Write</c>.</summary>
    FlagsText,
}
