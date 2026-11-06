using System.ComponentModel;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>Resource 03 of the sample API.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Status">A required enum.</param>
/// <param name="Stage">A nullable enum.</param>
/// <param name="Kind">An enum with a defined default.</param>
public sealed record Resource03(int Id, string Name, Enum07 Status, Enum08? Stage, [property: DefaultValue(Enum09.Beta)] Enum09 Kind);
