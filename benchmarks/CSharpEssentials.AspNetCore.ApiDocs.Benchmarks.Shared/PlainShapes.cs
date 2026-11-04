namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>
/// What a row without the enum conventions writes, asserted exactly: the <c>Enum01</c> component and status query, the
/// <c>Kind</c> default of every resource, and the nullable <c>Stage</c> property of <c>Resource01</c>.
/// </summary>
internal sealed record PlainShapes(string Enum01, string KindDefault, string Stage);
