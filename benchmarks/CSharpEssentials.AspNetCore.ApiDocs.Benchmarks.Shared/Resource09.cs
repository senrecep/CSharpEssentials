using System.ComponentModel;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>Resource 09 of the sample API.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Status">A required enum.</param>
/// <param name="Stage">A nullable enum.</param>
/// <param name="Kind">An enum with a defined default.</param>
public sealed record Resource09(int Id, string Name, Enum25 Status, Enum26? Stage, [property: DefaultValue(Enum27.Beta)] Enum27 Kind);
