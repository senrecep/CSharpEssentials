using System.ComponentModel;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>Resource 10 of the sample API.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Status">A required enum.</param>
/// <param name="Stage">A nullable enum.</param>
/// <param name="Kind">An enum with a defined default.</param>
public sealed record Resource10(int Id, string Name, Enum28 Status, Enum29? Stage, [property: DefaultValue(Enum30.Beta)] Enum30 Kind);
