namespace CSharpEssentials.Enums;

/// <summary>
/// The build-time naming of the wire names of a <see cref="StringEnumAttribute"/> enum.
/// </summary>
/// <remarks>
/// The separator styles follow <c>System.Text.Json.JsonNamingPolicy</c> exactly, so <c>HTTPStatus</c> is <c>http_status</c>.
/// </remarks>
public enum EnumNaming
{
    /// <summary>Use the project setting (<c>CSharpEssentialsEnumNaming</c>) or <see cref="SnakeCaseLower"/>.</summary>
    Default = 0,

    /// <summary><c>pending_approval</c></summary>
    SnakeCaseLower,

    /// <summary><c>PENDING_APPROVAL</c></summary>
    SnakeCaseUpper,

    /// <summary><c>pending-approval</c></summary>
    KebabCaseLower,

    /// <summary><c>PENDING-APPROVAL</c></summary>
    KebabCaseUpper,

    /// <summary><c>pendingApproval</c></summary>
    CamelCase,

    /// <summary><c>PendingApproval</c> (the member name).</summary>
    PascalCase,
}
