using System.ComponentModel;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>Resource 08 of the sample API.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Status">A required enum.</param>
/// <param name="Stage">A nullable enum.</param>
/// <param name="Kind">An enum with a defined default.</param>
public sealed record Resource08(int Id, string Name, Enum22 Status, Enum23? Stage, [property: DefaultValue(Enum24.Beta)] Enum24 Kind);
