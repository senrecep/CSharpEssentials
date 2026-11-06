using System.ComponentModel;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>Resource 07 of the sample API.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Status">A required enum.</param>
/// <param name="Stage">A nullable enum.</param>
/// <param name="Kind">An enum with a defined default.</param>
public sealed record Resource07(int Id, string Name, Enum19 Status, Enum20? Stage, [property: DefaultValue(Enum21.Beta)] Enum21 Kind);
